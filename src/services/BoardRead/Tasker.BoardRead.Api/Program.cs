using System.Text.Json;
using Cassandra;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using StackExchange.Redis;
using Tasker.Auth.Infrastructure;
using Tasker.BoardRead.Api.Security;
using Tasker.BoardRead.Application.Boards.Abstractions;
using Tasker.BoardRead.Application.Boards.Queries.GetMyBoards;
using Tasker.BoardRead.Application.Users.Abstractions;
using Tasker.BoardRead.Infrastructure.Boards;
using Tasker.BoardRead.Infrastructure.Users;
using Tasker.BoardWrite.Application.Abstractions.Security;
using Tasker.BoardWrite.Infrastructure;
using Tasker.BoardWrite.Infrastructure.Security;
using Tasker.Shared.Kernel.Abstractions;
using Tasker.Shared.Kernel.Abstractions.ReadModel;
using Tasker.Shared.Kernel.Abstractions.Security;
using Tasker.Shared.ReadModel.Cassandra;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext());

builder.Services.AddEndpointsApiExplorer();

// Swagger + Bearer auth
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Tasker BoardRead API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "Token",
        In = ParameterLocation.Header,
        Description = "Введите access-токен. Пример: 8CAEB2D5... (без 'Bearer ')"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddProblemDetails();
builder.Services.AddControllers();

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: new[] { "live" });

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(builder.Environment.ApplicationName))
    .WithMetrics(mb => mb
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

// ---------- Redis / Auth / CurrentUser / Access control ----------

var redisConn = builder.Configuration["Redis:Connection"] ?? "redis:6379";
builder.Services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(redisConn));

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

builder.Services.AddScoped<IAccessTokenValidator, RedisAccessTokenValidator>();

builder.Services
    .AddAuthentication(AccessTokenAuthenticationHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, AccessTokenAuthenticationHandler>(
        AccessTokenAuthenticationHandler.SchemeName,
        _ => { });

builder.Services.AddAuthorization();

// ---------- MySQL: BoardWriteDbContext (fallback для read) ----------
// TODO: Remove ts from here

var conn = builder.Configuration.GetConnectionString("BoardWrite")
           ?? builder.Configuration["ConnectionStrings:BoardWrite"]
           ?? "Server=mysql;Port=3306;Database=tasker;User=tasker;Password=tasker;TreatTinyAsBoolean=true;AllowUserVariables=true;DefaultCommandTimeout=30;";

var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<BoardWriteDbContext>(opt =>
{
    opt.UseMySql(conn, serverVersion,
            mySql => mySql.MigrationsHistoryTable("__EFMigrationsHistory", schema: null))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging();
});

// ---------- MySQL: AuthDbContext (чтение пользователей для View) ----------
// TODO: Remove ts from here

var authConn = builder.Configuration.GetConnectionString("Auth")
               ?? builder.Configuration["ConnectionStrings:Auth"]
               ?? "Server=mysql;Port=3306;Database=tasker;User=tasker;Password=tasker;TreatTinyAsBoolean=true;AllowUserVariables=true;DefaultCommandTimeout=30;";

builder.Services.AddDbContext<AuthDbContext>(opt =>
{
    opt.UseMySql(authConn, serverVersion,
            mySql => mySql.MigrationsHistoryTable("__EFMigrationsHistory", schema: null))
        .EnableDetailedErrors()
        .EnableSensitiveDataLogging();
});

// ---------- Cassandra: snapshots ----------

builder.Services.AddSingleton<Cassandra.ISession>(sp =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var host = cfg["Cassandra:Host"] ?? "cassandra";
    var portStr = cfg["Cassandra:Port"];
    var port = int.TryParse(portStr, out var p) ? p : 9042;

    var cluster = Cluster.Builder()
        .AddContactPoint(host)
        .WithPort(port)
        .Build();

    var session = cluster.Connect();

    session.Execute(@"
        CREATE KEYSPACE IF NOT EXISTS tasker_read
        WITH replication = { 'class': 'SimpleStrategy', 'replication_factor': 1 };
    ");

    session.Execute(@"
        CREATE TABLE IF NOT EXISTS tasker_read.board_snapshots (
            board_id uuid PRIMARY KEY,
            payload text
        );
    ");

    return session;
});

builder.Services.AddSingleton<IBoardSnapshotStore, CassandraBoardSnapshotStore>();

// ---------- Access service & read services ----------

builder.Services.AddScoped<IBoardAccessService, BoardAccessService>();
builder.Services.AddScoped<IBoardDetailsReadService, BoardDetailsReadService>();
builder.Services.AddScoped<IBoardListReadService, BoardListReadService>();
builder.Services.AddScoped<IUserReadService, AuthUserReadService>();

// MediatR
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(GetMyBoardsQuery).Assembly);
});

// CORS для фронта
const string frontendCorsPolicy = "FrontendDev";
builder.Services.AddCors(options =>
{
    options.AddPolicy(
        frontendCorsPolicy,
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseCors(frontendCorsPolicy);

app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapPrometheusScrapingEndpoint();

app.MapHealthChecks("/healthz", new HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live")
});

app.MapHealthChecks("/readyz", new HealthCheckOptions
{
    Predicate = _ => true,
    ResultStatusCodes =
    {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var payload = new
        {
            status = report.Status.ToString(),
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                durationMs = e.Value.Duration.TotalMilliseconds
            })
        };
        await ctx.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
});

app.MapGet("/healthz/quick", () => Results.Ok(new { status = "ok" }))
    .WithTags("system");

app.MapControllers();

app.Run();

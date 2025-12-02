using System;
using System.Linq;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Serilog;
using Tasker.BoardWrite.Application.Abstractions.Persistence;
using Tasker.BoardWrite.Application.Boards.Commands.CreateBoard;
using Tasker.BoardWrite.Infrastructure;
using Tasker.BoardWrite.Infrastructure.Persistence;
using Tasker.Shared.Kafka.Extensions;
using Tasker.Shared.Kernel.Abstractions;
using Tasker.Shared.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddProblemDetails();

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

builder.Services.AddKafkaCore(builder.Configuration);

var conn = builder.Configuration.GetConnectionString("BoardWrite")
           ?? builder.Configuration["ConnectionStrings:BoardWrite"]
           ?? "Server=mysql;Port=3306;Database=tasker;User=tasker;Password=tasker;TreatTinyAsBoolean=true;AllowUserVariables=true;DefaultCommandTimeout=30;";

var serverVersion = new MySqlServerVersion(new Version(8, 0, 36));

builder.Services.AddDbContext<BoardWriteDbContext>(opt =>
{
    opt.UseMySql(conn, serverVersion,
        mySql => mySql.MigrationsHistoryTable("__EFMigrationsHistory", schema: null));
});

builder.Services.AddScoped<IBoardRepository, BoardRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssemblies(typeof(CreateBoardCommand).Assembly);
});

builder.Services.AddControllers(options =>
{
    options.Filters.Add<AppExceptionFilter>();
});

var app = builder.Build();

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

static bool IsEfDesignTime() =>
    AppDomain.CurrentDomain.GetAssemblies()
        .Any(a => a.FullName?.StartsWith("Microsoft.EntityFrameworkCore.Design", StringComparison.OrdinalIgnoreCase) == true);

if (!IsEfDesignTime())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BoardWriteDbContext>();
    db.Database.Migrate();
}

app.Run();

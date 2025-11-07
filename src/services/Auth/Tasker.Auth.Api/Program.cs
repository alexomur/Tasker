using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using Tasker.Auth.Application.Abstractions.Persistence;
using Tasker.Auth.Infrastructure;
using Tasker.Auth.Infrastructure.Persistence;
using Tasker.Shared.Kafka.Extensions;

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

var conn = builder.Configuration.GetConnectionString("Auth")
           ?? builder.Configuration["ConnectionStrings:Auth"] 
           ?? "Server=mysql;Port=3306;Database=tasker;User=tasker;Password=dev;TreatTinyAsBoolean=true;AllowUserVariables=true;DefaultCommandTimeout=30;";
builder.Services.AddDbContext<AuthDbContext>(opt =>
{
    opt.UseMySql(conn, ServerVersion.AutoDetect(conn),
        mySql => mySql.MigrationsHistoryTable("__EFMigrationsHistory", schema: null));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

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

app.MapHealthChecks("/healthz", new HealthCheckOptions {
    Predicate = r => r.Tags.Contains("live")
});

app.MapHealthChecks("/readyz", new HealthCheckOptions {
    Predicate = _ => true,
    ResultStatusCodes = {
        [HealthStatus.Healthy] = StatusCodes.Status200OK,
        [HealthStatus.Degraded] = StatusCodes.Status200OK,
        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
    },
    ResponseWriter = async (ctx, report) =>
    {
        ctx.Response.ContentType = "application/json";
        var payload = new {
            status = report.Status.ToString(),
            entries = report.Entries.Select(e => new {
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

static bool IsEfDesignTime() =>
    AppDomain.CurrentDomain.GetAssemblies()
        .Any(a => a.FullName?.StartsWith("Microsoft.EntityFrameworkCore.Design", StringComparison.OrdinalIgnoreCase) == true);

if (!IsEfDesignTime())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

app.Run();
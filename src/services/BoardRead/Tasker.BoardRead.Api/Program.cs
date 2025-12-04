using System.Text.Json;
using Cassandra;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using Serilog;
using Tasker.Shared.Kernel.Abstractions.ReadModel;
using Tasker.Shared.ReadModel.Cassandra;

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

builder.Services.AddSingleton<global::Cassandra.ISession>(sp =>
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

    // На всякий случай убедимся, что keyspace и таблица есть
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

const int boardSnapshotTtlSeconds = 24 * 60 * 60; // 24 часа
app.MapGet("/api/v1/boards/{boardId:guid}", async (Guid boardId, IBoardSnapshotStore snapshots) =>
{
    var json = await snapshots.TryGetAsync(boardId);
    if (json is null)
    {
        return Results.NotFound(new { message = "Board snapshot not found" });
    }

    await snapshots.UpsertAsync(boardId, json, boardSnapshotTtlSeconds);

    return Results.Content(json, "application/json");
});

app.Run();
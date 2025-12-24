using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tasker.AnalyticsIngest.Worker;
using Tasker.Shared.Kafka;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
        config.AddEnvironmentVariables();
    })
    .ConfigureServices((context, services) =>
    {
        services.Configure<KafkaOptions>(context.Configuration.GetSection("Kafka"));
        services.Configure<HdfsOptions>(context.Configuration.GetSection("Hdfs"));
        services.Configure<IngestOptions>(context.Configuration.GetSection("Ingest"));

        services
            .AddHttpClient<HdfsWebClient>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                AllowAutoRedirect = false
            });
        services.AddHostedService<KafkaHdfsIngestWorker>();
    });

await builder.RunConsoleAsync();

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Tasker.Messaging.Kafka.Interfaces;

namespace Tasker.Messaging.Kafka.Extensions;

public static class ServerCollectionExtensions
{
    public static IServiceCollection AddKafkaCore(this IServiceCollection services, IConfiguration cfg)
    {
        services.Configure<KafkaOptions>(cfg.GetSection("Kafka"));
        services.AddSingleton<IEventProducer, KafkaProducer>();
        return services;
    }
    
    public static IServiceCollection AddKafkaConsumer<THandler>(this IServiceCollection services, string topic, string groupId) where THandler : class, IEventConsumerHandler
    {
        services.AddSingleton<IEventConsumerHandler, THandler>();
        services.AddSingleton<IHostedService>(sp =>
        {
            var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<KafkaOptions>>();
            var handler = sp.GetRequiredService<IEventConsumerHandler>();
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<KafkaConsumerHostedService>>();
            return new KafkaConsumerHostedService(options, handler, logger, topic, groupId);
        });
        return services;
    }
}
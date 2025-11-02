using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tasker.Messaging.Kafka.Interfaces;

namespace Tasker.Messaging.Kafka;

internal sealed class KafkaConsumerHostedService : BackgroundService
{
    private readonly IConsumer<string, byte[]> _consumer;

    private readonly IEventConsumerHandler _handler;
    
    private readonly ILogger<KafkaConsumerHostedService> _logger;

    private readonly string _topic;

    public KafkaConsumerHostedService(IOptions<KafkaOptions> rawOptions, IEventConsumerHandler handler, ILogger<KafkaConsumerHostedService> logger, string topic, string groupId)
    {
        _handler = handler;
        _logger = logger;
        _topic = topic;
        
        var options = rawOptions.Value;
        var config = new ConsumerConfig
        {
            BootstrapServers = options.Brokers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _consumer = new ConsumerBuilder<string, byte[]>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);
        _logger.LogInformation("Kafka consumer subscribed to {Topic}", _topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var result = _consumer.Consume(stoppingToken);
                await _handler.HandleAsync(result.Topic, result.Message.Key ?? "", result.Message.Value, stoppingToken);
                _consumer.Commit(result);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kafka consumer crashed");
        }
        finally
        {
            _consumer.Close();
        }
    }
}
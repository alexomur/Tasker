using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tasker.Messaging.Kafka.Interfaces;

namespace Tasker.Messaging.Kafka;

internal class KafkaProducer : IEventProducer, IDisposable
{
    private readonly IProducer<string, byte[]> _producer;
    
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IOptions<KafkaOptions> rawOptions, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var options = rawOptions.Value;
        var config = new ProducerConfig
        {
            BootstrapServers = options.Brokers,
            ClientId = options.ClientId ?? Environment.MachineName,
            EnableIdempotence = options.EnableIdempotence,
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<string, byte[]>(config).Build();
    }
    
    public async Task ProduceAsync(string topic, string key, ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default)
    {
        var message = new Message<string, byte[]>
        {
            Key = key,
            Value = value.ToArray(),
        };
        var result = await _producer.ProduceAsync(topic, message, cancellationToken);
        _logger.LogDebug("Produced to {Topic} @{Partition}:{Offset}", result.Topic, result.Partition, result.Offset);
    }
    
    public void Dispose()
    {
        _producer?.Dispose();
    }
}
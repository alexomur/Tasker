namespace Tasker.Messaging.Kafka.Interfaces;

public interface IEventProducer
{
    Task ProduceAsync(string topic, string key, ReadOnlyMemory<byte> value, CancellationToken cancellationToken = default);
}
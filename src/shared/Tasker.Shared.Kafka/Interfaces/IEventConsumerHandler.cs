namespace Tasker.Shared.Kafka.Interfaces;

public interface IEventConsumerHandler
{
    Task HandleAsync(string topic, string key, ReadOnlyMemory<byte> value, CancellationToken cancellationToken);
}
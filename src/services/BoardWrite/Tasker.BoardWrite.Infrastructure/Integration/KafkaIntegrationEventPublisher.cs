using System.Text.Json;
using Tasker.Shared.Kafka.Interfaces;

namespace Tasker.BoardWrite.Infrastructure.Integration;

public interface IIntegrationEventPublisher
{
    Task PublishAsync(IntegrationMessage message, CancellationToken cancellationToken = default);
}

public sealed class KafkaIntegrationEventPublisher : IIntegrationEventPublisher
{
    private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly IEventProducer _producer;

    public KafkaIntegrationEventPublisher(IEventProducer producer)
    {
        _producer = producer;
    }

    public Task PublishAsync(IntegrationMessage message, CancellationToken cancellationToken = default)
    {
        var payloadType = message.Payload.GetType();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(message.Payload, payloadType, SerializerOptions);

        return _producer.ProduceAsync(
            topic: message.Topic,
            key: message.Key,
            value: bytes,
            cancellationToken: cancellationToken);
    }
}
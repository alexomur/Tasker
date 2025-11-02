namespace Tasker.Messaging.Kafka;

public sealed class KafkaOptions
{
    public string Brokers { get; set; } = "redpanda:9092";
    
    public string? ClientId { get; set; }
    
    public bool EnableIdempotence { get; set; } = true;
}

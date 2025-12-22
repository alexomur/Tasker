using Confluent.Kafka;
using System.Text;
using System.Text.Json;

namespace Tasker.AnalyticsIngest.Worker;

public sealed class BufferedMessage
{
    public ConsumeResult<string, byte[]> Source { get; }

    public AnalyticsEnvelope Envelope { get; }

    public DateTimeOffset PartitionTime { get; }

    private BufferedMessage(ConsumeResult<string, byte[]> source, AnalyticsEnvelope envelope, DateTimeOffset partitionTime)
    {
        Source = source;
        Envelope = envelope;
        PartitionTime = partitionTime;
    }

    public static BufferedMessage From(ConsumeResult<string, byte[]> result)
    {
        var payloadText = result.Message.Value is null
            ? string.Empty
            : Encoding.UTF8.GetString(result.Message.Value);

        JsonElement? payload = null;
        string? payloadRaw = null;

        if (!string.IsNullOrWhiteSpace(payloadText))
        {
            try
            {
                using var doc = JsonDocument.Parse(payloadText);
                payload = doc.RootElement.Clone();
            }
            catch
            {
                payloadRaw = payloadText;
            }
        }

        var timestamp = result.Message.Timestamp.Type == TimestampType.NotAvailable
            ? DateTimeOffset.UtcNow
            : DateTimeOffset.FromUnixTimeMilliseconds(result.Message.Timestamp.UnixTimestampMs);

        var envelope = new AnalyticsEnvelope(
            Topic: result.Topic,
            Key: result.Message.Key ?? string.Empty,
            Partition: result.Partition.Value,
            Offset: result.Offset.Value,
            Timestamp: timestamp,
            Payload: payload,
            PayloadRaw: payloadRaw);

        var partitionTime = timestamp;

        return new BufferedMessage(result, envelope, partitionTime);
    }
}

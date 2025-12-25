using System.Text.Json;

namespace Tasker.AnalyticsIngest.Worker;

public sealed record AnalyticsEnvelope(
    string Topic,
    string Key,
    int Partition,
    long Offset,
    DateTimeOffset Timestamp,
    JsonElement? Payload,
    string? PayloadRaw
);

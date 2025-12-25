namespace Tasker.AnalyticsIngest.Worker;

public sealed class IngestOptions
{
    public string[] Topics { get; set; } = Array.Empty<string>();

    public string GroupId { get; set; } = "analytics-hdfs-ingest";

    public int BatchSize { get; set; } = 200;

    public int FlushIntervalSeconds { get; set; } = 5;

    public int MaxBufferSize { get; set; } = 5000;

    public int RetryDelaySeconds { get; set; } = 5;
}

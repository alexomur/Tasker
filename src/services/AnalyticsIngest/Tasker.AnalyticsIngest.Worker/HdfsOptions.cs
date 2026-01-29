namespace Tasker.AnalyticsIngest.Worker;

public sealed class HdfsOptions
{
    public string WebHdfsBaseUrl { get; set; } = "http://namenode:9870";

    public string BasePath { get; set; } = "/raw/events";

    public int RequestTimeoutSeconds { get; set; } = 30;
}

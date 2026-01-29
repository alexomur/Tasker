using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tasker.Shared.Kafka;

namespace Tasker.AnalyticsIngest.Worker;

public sealed class KafkaHdfsIngestWorker : BackgroundService
{
    private readonly KafkaOptions _kafkaOptions;
    private readonly IngestOptions _ingestOptions;
    private readonly HdfsOptions _hdfsOptions;
    private readonly HdfsWebClient _hdfsClient;
    private readonly ILogger<KafkaHdfsIngestWorker> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public KafkaHdfsIngestWorker(
        IOptions<KafkaOptions> kafkaOptions,
        IOptions<IngestOptions> ingestOptions,
        IOptions<HdfsOptions> hdfsOptions,
        HdfsWebClient hdfsClient,
        ILogger<KafkaHdfsIngestWorker> logger)
    {
        _kafkaOptions = kafkaOptions.Value;
        _ingestOptions = ingestOptions.Value;
        _hdfsOptions = hdfsOptions.Value;
        _hdfsClient = hdfsClient;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_ingestOptions.Topics.Length == 0)
        {
            _logger.LogWarning("No Kafka topics configured for HDFS ingestion.");
            return;
        }

        var flushInterval = TimeSpan.FromSeconds(_ingestOptions.FlushIntervalSeconds);
        var retryDelay = TimeSpan.FromSeconds(_ingestOptions.RetryDelaySeconds);

        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaOptions.Brokers,
            GroupId = _ingestOptions.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false,
            ClientId = _kafkaOptions.ClientId ?? $"analytics-ingest-{Environment.MachineName}",
            AllowAutoCreateTopics = true
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            using var consumer = new ConsumerBuilder<string, byte[]>(config).Build();
            consumer.Subscribe(_ingestOptions.Topics);
            _logger.LogInformation("Kafka ingest consumer subscribed to: {Topics}", string.Join(", ", _ingestOptions.Topics));

            var buffer = new List<ConsumeResult<string, byte[]>>();
            var lastFlush = DateTimeOffset.UtcNow;

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<string, byte[]>? result = null;
                    try
                    {
                        result = consumer.Consume(TimeSpan.FromMilliseconds(500));
                    }
                    catch (ConsumeException ex) when (!ex.Error.IsFatal)
                    {
                        _logger.LogWarning(ex,
                            "Kafka consume error ({Code}); retrying in {DelaySeconds}s.",
                            ex.Error.Code,
                            retryDelay.TotalSeconds);
                        await Task.Delay(retryDelay, stoppingToken);
                        continue;
                    }

                    if (result is not null)
                    {
                        buffer.Add(result);
                    }

                    var dueToFlush = buffer.Count >= _ingestOptions.BatchSize
                                     || buffer.Count >= _ingestOptions.MaxBufferSize
                                     || (buffer.Count > 0 && DateTimeOffset.UtcNow - lastFlush >= flushInterval);

                    if (!dueToFlush)
                    {
                        continue;
                    }

                    if (buffer.Count == 0)
                    {
                        lastFlush = DateTimeOffset.UtcNow;
                        continue;
                    }

                    var flushed = await FlushAsync(buffer, stoppingToken);
                    if (flushed)
                    {
                        CommitOffsets(consumer, buffer);
                        buffer.Clear();
                        lastFlush = DateTimeOffset.UtcNow;
                    }
                    else
                    {
                        _logger.LogWarning("Flush failed; retrying in {DelaySeconds}s.", retryDelay.TotalSeconds);
                        await Task.Delay(retryDelay, stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ConsumeException ex) when (ex.Error.IsFatal)
            {
                _logger.LogError(ex, "Kafka consumer fatal error; restarting in {DelaySeconds}s.", retryDelay.TotalSeconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka ingest worker crashed; restarting in {DelaySeconds}s.", retryDelay.TotalSeconds);
            }
            finally
            {
                consumer.Close();
            }

            if (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(retryDelay, stoppingToken);
            }
        }
    }

    private async Task<bool> FlushAsync(List<ConsumeResult<string, byte[]>> buffer, CancellationToken cancellationToken)
    {
        var prepared = buffer
            .Select(BufferedMessage.From)
            .ToList();

        var groups = prepared.GroupBy(item => new
        {
            item.Envelope.Topic,
            PartitionTime = new DateTimeOffset(
                item.PartitionTime.Year,
                item.PartitionTime.Month,
                item.PartitionTime.Day,
                item.PartitionTime.Hour,
                0,
                0,
                TimeSpan.Zero)
        });

        try
        {
            foreach (var group in groups)
            {
                var directory = BuildDirectoryPath(group.Key.Topic, group.Key.PartitionTime);
                await _hdfsClient.EnsureDirectoryAsync(directory, cancellationToken);

                var fileName = $"part-{group.Key.PartitionTime:yyyyMMddHHmmss}-{Guid.NewGuid():N}.json";
                var filePath = $"{directory}/{fileName}";

                var jsonLines = group
                    .Select(item => JsonSerializer.Serialize(item.Envelope, JsonOptions))
                    .ToArray();

                var payload = string.Join("\n", jsonLines) + "\n";
                var bytes = Encoding.UTF8.GetBytes(payload);

                await _hdfsClient.CreateFileAsync(filePath, bytes, "application/json", cancellationToken);
            }

            _logger.LogInformation("Flushed {Count} Kafka messages to HDFS.", buffer.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to flush Kafka messages to HDFS.");
            return false;
        }
    }

    private string BuildDirectoryPath(string topic, DateTimeOffset partitionTime)
    {
        var basePath = _hdfsOptions.BasePath.TrimEnd('/');
        var date = partitionTime.ToString("yyyy-MM-dd");
        var hour = partitionTime.ToString("HH");
        return $"{basePath}/{topic}/dt={date}/hour={hour}";
    }

    private static void CommitOffsets(IConsumer<string, byte[]> consumer, List<ConsumeResult<string, byte[]>> buffer)
    {
        var offsets = buffer
            .GroupBy(result => result.TopicPartition)
            .Select(group =>
            {
                var lastOffset = group.Max(r => r.Offset.Value);
                return new TopicPartitionOffset(group.Key, new Offset(lastOffset + 1));
            })
            .ToList();

        consumer.Commit(offsets);
    }
}

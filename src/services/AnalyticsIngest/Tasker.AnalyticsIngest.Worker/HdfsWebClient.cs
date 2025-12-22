using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Tasker.AnalyticsIngest.Worker;

public sealed class HdfsWebClient
{
    private readonly HttpClient _httpClient;
    private readonly HdfsOptions _options;
    private readonly ILogger<HdfsWebClient> _logger;
    private readonly ConcurrentDictionary<string, bool> _createdDirectories = new(StringComparer.Ordinal);

    public HdfsWebClient(HttpClient httpClient, IOptions<HdfsOptions> options, ILogger<HdfsWebClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        _httpClient.Timeout = TimeSpan.FromSeconds(_options.RequestTimeoutSeconds);
    }

    public async Task EnsureDirectoryAsync(string path, CancellationToken cancellationToken)
    {
        var normalized = NormalizePath(path);
        if (!_createdDirectories.TryAdd(normalized, true))
        {
            return;
        }

        var uri = BuildUri(normalized, "MKDIRS");
        using var request = new HttpRequestMessage(HttpMethod.Put, uri);
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _createdDirectories.TryRemove(normalized, out _);
            var payload = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Failed to create HDFS directory '{normalized}': {(int)response.StatusCode} {response.ReasonPhrase}. {payload}");
        }
    }

    public async Task CreateFileAsync(string path, byte[] content, string contentType, CancellationToken cancellationToken)
    {
        var normalized = NormalizePath(path);
        var createUri = BuildUri(normalized, "CREATE", "overwrite=false");

        using var createRequest = new HttpRequestMessage(HttpMethod.Put, createUri);
        using var createResponse = await _httpClient.SendAsync(createRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (createResponse.StatusCode == HttpStatusCode.TemporaryRedirect || createResponse.StatusCode == HttpStatusCode.PermanentRedirect)
        {
            var location = createResponse.Headers.Location;
            if (location is null)
            {
                throw new InvalidOperationException($"WebHDFS redirect missing Location header for '{normalized}'.");
            }

            using var uploadRequest = new HttpRequestMessage(HttpMethod.Put, location);
            uploadRequest.Content = new ByteArrayContent(content);
            uploadRequest.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            using var uploadResponse = await _httpClient.SendAsync(uploadRequest, cancellationToken);
            if (!uploadResponse.IsSuccessStatusCode)
            {
                var payload = await uploadResponse.Content.ReadAsStringAsync(cancellationToken);
                throw new InvalidOperationException(
                    $"Failed to upload HDFS file '{normalized}': {(int)uploadResponse.StatusCode} {uploadResponse.ReasonPhrase}. {payload}");
            }

            return;
        }

        if (createResponse.StatusCode == HttpStatusCode.Created)
        {
            _logger.LogInformation("Created HDFS file '{Path}' without redirect.", normalized);
            return;
        }

        var body = await createResponse.Content.ReadAsStringAsync(cancellationToken);
        throw new InvalidOperationException(
            $"Failed to create HDFS file '{normalized}': {(int)createResponse.StatusCode} {createResponse.ReasonPhrase}. {body}");
    }

    private Uri BuildUri(string path, string operation, string? extraQuery = null)
    {
        var baseUrl = _options.WebHdfsBaseUrl.TrimEnd('/');
        var encodedPath = EncodePath(path);
        var uri = $"{baseUrl}/webhdfs/v1{encodedPath}?op={operation}";
        if (!string.IsNullOrWhiteSpace(extraQuery))
        {
            uri = $"{uri}&{extraQuery}";
        }

        return new Uri(uri);
    }

    private static string NormalizePath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        var normalized = path.Replace('\\', '/').Trim();
        if (!normalized.StartsWith("/", StringComparison.Ordinal))
        {
            normalized = "/" + normalized;
        }

        return normalized;
    }

    private static string EncodePath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length == 0)
        {
            return "/";
        }

        var encodedSegments = segments.Select(Uri.EscapeDataString);
        return "/" + string.Join("/", encodedSegments);
    }
}

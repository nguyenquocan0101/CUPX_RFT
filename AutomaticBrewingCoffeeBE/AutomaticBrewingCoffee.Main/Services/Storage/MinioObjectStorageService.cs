using System.Net;
using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using Services.Supabase;

namespace Services.Storage;

public sealed class MinioOptions
{
    public string Endpoint { get; set; } = "http://127.0.0.1:9000";
    public string AccessKey { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string Bucket { get; set; } = string.Empty;
    public string PublicBaseUrl { get; set; } = string.Empty;
    public string PublicEndpoint { get; set; } = string.Empty;
    public bool UsePathStyle { get; set; } = true;
}

public interface IObjectStorageService
{
    Task<string> UploadFile(byte[] fileByte, string filePath, string bucketName, bool replace);
    string RetrievePublicUrl(string bucketName, string filePath);
    Task<byte[]> DownloadFile(string bucketName, string pathOrUrl);
    bool IsObjectStorageResource(string imageUrl);
}

public sealed class MinioObjectStorageService : IObjectStorageService, ISupabaseStorageService
{
    private readonly MinioOptions _options;
    private readonly IMinioClient _client;

    public MinioObjectStorageService(MinioOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        var endpoint = new Uri(_options.Endpoint);
        var builder = new MinioClient()
            .WithEndpoint(endpoint.Host, endpoint.Port)
            .WithCredentials(_options.AccessKey, _options.SecretKey);

        if (endpoint.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
            builder = builder.WithSSL();

        _client = builder.Build();
    }

    public MinioObjectStorageService(IOptions<MinioOptions> options)
        : this(options?.Value ?? throw new ArgumentNullException(nameof(options)))
    {
    }

    public async Task<string> UploadFile(byte[] fileByte, string filePath, string bucketName, bool replace)
    {
        ArgumentNullException.ThrowIfNull(fileByte);
        var bucket = ResolveBucket(bucketName);

        await using var stream = new MemoryStream(fileByte, writable: false);
        await _client.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucket)
            .WithObject(NormalizePath(filePath))
            .WithStreamData(stream)
            .WithObjectSize(fileByte.Length)
            .WithContentType(ResolveContentType(filePath)), CancellationToken.None);

        return NormalizePath(filePath);
    }

    public string RetrievePublicUrl(string bucketName, string filePath)
    {
        var bucket = ResolveBucket(bucketName);
        var baseUrl = !string.IsNullOrWhiteSpace(_options.PublicBaseUrl)
            ? _options.PublicBaseUrl.TrimEnd('/')
            : (string.IsNullOrWhiteSpace(_options.PublicEndpoint)
                ? _options.Endpoint.TrimEnd('/') + "/" + bucket
                : _options.PublicEndpoint.TrimEnd('/') + "/" + bucket);
        return $"{baseUrl}/{NormalizePath(filePath)}";
    }

    public async Task<byte[]> DownloadFile(string bucketName, string pathOrUrl)
    {
        var bucket = ResolveBucket(bucketName);
        var objectPath = ExtractObjectPath(bucket, pathOrUrl);
        await using var output = new MemoryStream();
        await _client.GetObjectAsync(new GetObjectArgs()
            .WithBucket(bucket)
            .WithObject(objectPath)
            .WithCallbackStream(stream => stream.CopyTo(output)), CancellationToken.None);
        return output.ToArray();
    }

    public bool IsObjectStorageResource(string imageUrl) => IsLocalEndpoint(imageUrl);

    public bool IsSupabaseResource(string imageUrl) => IsObjectStorageResource(imageUrl);

    private string ResolveBucket(string bucketName) =>
        string.IsNullOrWhiteSpace(_options.Bucket) ? bucketName : _options.Bucket;

    private bool IsLocalEndpoint(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return false;
        var configured = new[] { _options.Endpoint, _options.PublicBaseUrl }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => Uri.TryCreate(x, UriKind.Absolute, out var parsed) ? parsed : null)
            .Where(x => x is not null)
            .Cast<Uri>();
        return configured.Any(x => string.Equals(x.Host, uri.Host, StringComparison.OrdinalIgnoreCase)
            && x.Port == uri.Port);
    }

    private string ExtractObjectPath(string bucket, string pathOrUrl)
    {
        if (!Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var uri))
            return NormalizePath(pathOrUrl);
        var path = uri.AbsolutePath.Trim('/');
        var prefix = bucket + "/";
        return path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
            ? path[prefix.Length..]
            : path;
    }

    private static string NormalizePath(string path) => path.Trim().Trim('/').Replace('\\', '/');

    private static string ResolveContentType(string path) =>
        Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
}

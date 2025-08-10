using Amazon.S3;
using Amazon.S3.Model;

using ChatApp.Application.Abstractions.Storage;

using Microsoft.Extensions.Options;

namespace ChatApp.Infrastructure.Storage;

internal class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly AmazonS3Settings _settings;

    public S3FileStorageService(IAmazonS3 s3, IOptions<AmazonS3Settings> options)
    {
        _s3 = s3;
        _settings = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task Upload(Stream stream, string key, string contentType, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            InputStream = stream,
            ContentType = contentType
        };

        await _s3.PutObjectAsync(request, cancellationToken);
    }


    public string GeneratePresignedUrl(string key, TimeSpan expiration)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _settings.BucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiration)
        };

        return _s3.GetPreSignedURL(request);
    }

    public async Task Delete(string key, CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _settings.BucketName,
            Key = key
        };

        await _s3.DeleteObjectAsync(request, cancellationToken);
    }
}

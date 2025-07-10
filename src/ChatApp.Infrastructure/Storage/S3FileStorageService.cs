using Amazon.S3;
using Amazon.S3.Model;

using ChatApp.Application.Abstractions.Storage;

using Microsoft.Extensions.Configuration;

namespace ChatApp.Infrastructure.Storage;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string _bucket;

    public S3FileStorageService(IAmazonS3 s3, IConfiguration config)
    {
        _s3 = s3;
        _bucket = config["AwsS3:BucketName"] ?? throw new ArgumentNullException("AwsS3:BucketName", "S3 bucket name configuration is missing.");
    }

    public async Task Upload(Stream stream, string key, string contentType, CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucket,
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
            BucketName = _bucket,
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
            BucketName = _bucket,
            Key = key
        };

        await _s3.DeleteObjectAsync(request, cancellationToken);
    }
}

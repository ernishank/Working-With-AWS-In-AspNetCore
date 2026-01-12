using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using WorkingWithS3.Models.DTO;

namespace WorkingWithS3.Endpoints;

public static class FileEndpoints
{
    public static void MapFileEndpoints(this WebApplication app)
    {
        var fileGroup = app.MapGroup("/api/files")
            .WithTags("Files");

        fileGroup.MapPost("/upload", async (IAmazonS3 s3Client, IFormFile formFile, string bucketName, string? prefix) =>
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (!bucketExists)
                return Results.NotFound($"Bucket {bucketName} does not exist.");

            var request = new PutObjectRequest()
            {
                BucketName = bucketName,
                Key = string.IsNullOrEmpty(prefix) ? formFile.FileName : $"{prefix?.TrimEnd('/')}/{formFile.FileName}",
                InputStream = formFile.OpenReadStream()
            };
            request.Metadata.Add("Content-Type", formFile.ContentType);
            await s3Client.PutObjectAsync(request);
            return Results.Ok($"File {prefix}/{formFile.FileName} uploaded to S3 successfully!");
        });

        fileGroup.MapGet("/", async (IAmazonS3 s3Client, string bucketName, string? prefix) =>
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (!bucketExists)
                return Results.NotFound($"Bucket {bucketName} does not exist.");

            var request = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Prefix = prefix
            };
            var result = await s3Client.ListObjectsV2Async(request);
            var s3Objects = result.S3Objects.Select(s =>
            {
                var urlRequest = new GetPreSignedUrlRequest()
                {
                    BucketName = bucketName,
                    Key = s.Key,
                    Expires = DateTime.UtcNow.AddMinutes(1)
                };
                return new S3ObjectDto()
                {
                    Name = s.Key.ToString(),
                    PresignedUrl = s3Client.GetPreSignedURL(urlRequest),
                };
            });
            return Results.Ok(s3Objects);
        });

        fileGroup.MapGet("/download", async (IAmazonS3 s3Client, string bucketName, string key) =>
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (!bucketExists)
                return Results.NotFound($"Bucket {bucketName} does not exist.");

            var s3Object = await s3Client.GetObjectAsync(bucketName, key);
            return Results.File(s3Object.ResponseStream, s3Object.Headers.ContentType);
        });

        fileGroup.MapDelete("/", async (IAmazonS3 s3Client, string bucketName, string key) =>
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (!bucketExists)
                return Results.NotFound($"Bucket {bucketName} does not exist.");

            await s3Client.DeleteObjectAsync(bucketName, key);
            return Results.NoContent();
        });
    }
}


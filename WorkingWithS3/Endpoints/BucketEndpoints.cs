using Amazon.S3;
using Amazon.S3.Util;

namespace WorkingWithS3.Endpoints;

public static class BucketEndpoints
{
    public static void MapBucketEndpoints(this WebApplication app)
    {
        var bucketGroup = app.MapGroup("/api/buckets")
            .WithTags("Buckets");

        bucketGroup.MapPost("/", async (IAmazonS3 s3Client, string bucketName) =>
        {
            var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(s3Client, bucketName);
            if (!bucketExists)
            {
                var response = await s3Client.PutBucketAsync(bucketName);
                if (response != null)
                {
                    return Results.Created($"/api/buckets/{bucketName}", new { BucketName = bucketName });
                }
            }
            return Results.BadRequest("Bucket already exists or an error occurred");
        });

        bucketGroup.MapGet("/", async (IAmazonS3 s3Client) =>
        {
            var list = await s3Client.ListBucketsAsync();
            var bucketNames = list.Buckets.Select(b => b.BucketName).ToList();
            return bucketNames.Any() ? Results.Ok(bucketNames) : Results.NotFound("No buckets found");
        });

        bucketGroup.MapDelete("/{bucketName}", async (IAmazonS3 s3Client, string bucketName) =>
        {
            await s3Client.DeleteBucketAsync(bucketName);
            return Results.NoContent();
        });
    }
}


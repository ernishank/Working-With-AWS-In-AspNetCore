using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using System.Text.Json;

namespace AmazonRekognitionApp.API
{
    public static class RekognitionEndpoints
    {
        public static void MapRekognitionEndpoints(this WebApplication application)
        {
            var rekognitionGroup = application.MapGroup("api/rekognition")
                .WithTags("Rekognition Service");

            rekognitionGroup.MapPost("/detect-labels", async (IFormFile file, IAmazonRekognition client) =>
            {
                var memStream = new MemoryStream();
                file.CopyTo(memStream);
                var response = await client.DetectLabelsAsync(new DetectLabelsRequest()
                {
                    Image = new Amazon.Rekognition.Model.Image()
                    {
                        Bytes = memStream
                    },
                    MaxLabels = 20,
                    MinConfidence = 70
                });
                Console.WriteLine(JsonSerializer.Serialize(response));
                var labels = new List<string>();
                foreach (var label in response.Labels)
                {
                    labels.Add(label.Name);
                }
                return Results.Ok(labels);
            }).DisableAntiforgery();
        }
    }
}

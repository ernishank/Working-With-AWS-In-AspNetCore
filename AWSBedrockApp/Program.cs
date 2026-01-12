using Amazon;
using Amazon.BedrockRuntime;
using Amazon.Runtime;
using Amazon.Runtime.Credentials;
using AWSBedrockApp.Models.Cohere;
using AWSBedrockApp.Models.DTO;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<AmazonBedrockRuntimeClient>(_ =>
{
    var credentials = DefaultAWSCredentialsIdentityResolver.GetCredentials();
    var config = new AmazonBedrockRuntimeConfig
    {
        RegionEndpoint = RegionEndpoint.USEast1  // Change to your preferred region
    };
    return new AmazonBedrockRuntimeClient(credentials, config);
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapPost("/prompts/text", async (AmazonBedrockRuntimeClient client, TextPromptRequest request) =>
{
    var coherePrompt = new CoherePrompt(request.Prompt);
    var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(coherePrompt));
    var stream = new MemoryStream(bytes);
    var requestModel = new Amazon.BedrockRuntime.Model.InvokeModelRequest()
    {
        ModelId = "cohere.command-text-v14",
        ContentType = "application/json",
        Accept = "*/*",
        Body = stream
    };
    var response = await client.InvokeModelAsync(requestModel);
    var data = JsonSerializer.Deserialize<CohereResponse>(response.Body);
    return new TextPromptReponse(data!.Generations![0].Text!.Trim());
});

app.Run();



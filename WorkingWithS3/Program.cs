using System.Diagnostics;
using Amazon.S3;
using WorkingWithS3.Endpoints;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonS3>();

var app = builder.Build();

// Inline middleware - Request logging with timing
app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
    var stopwatch = Stopwatch.StartNew();
    
    logger.LogInformation("➡️ {Method} {Path} started", 
        context.Request.Method, 
        context.Request.Path);
    
    await next(context);
    
    stopwatch.Stop();
    logger.LogInformation("⬅️ {Method} {Path} completed with {StatusCode} in {ElapsedMs}ms",
        context.Request.Method,
        context.Request.Path,
        context.Response.StatusCode,
        stopwatch.ElapsedMilliseconds);
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

// Map API endpoints
app.MapBucketEndpoints();
app.MapFileEndpoints();

app.Run();

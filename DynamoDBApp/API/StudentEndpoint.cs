using Amazon.DynamoDBv2.DataModel;
using Amazon.Runtime.Internal;
using DynamoDBApp.Models;

namespace DynamoDBApp.API
{
    public static class StudentEndpoint
    {
        public static void StudentEndpoints(this WebApplication app)
        {
            var studentGroup = app.MapGroup("api/student").WithTags("Student");

            studentGroup.MapGet("/{studentId}", async (int studentId, IDynamoDBContext dynamodbContext) =>
            {
                var student = await dynamodbContext.LoadAsync<Student>(studentId);
                if (student != null)
                {
                    return Results.Ok(student);
                }
                return Results.NotFound("Student does not exists.");
            });

            studentGroup.MapGet("", async (IDynamoDBContext dynamodbContext) =>
            {
                var result = await dynamodbContext.ScanAsync<Student>(new List<ScanCondition>()).GetRemainingAsync();
                return Results.Ok(result);
            });

            studentGroup.MapPost("", async (IDynamoDBContext dbContext, Student request) =>
            {
                var student = await dbContext.LoadAsync<Student>(request.Id);
                if (student == null)
                {
                    await dbContext.SaveAsync<Student>(request);
                    return Results.Created();
                }
                return Results.BadRequest("Student already exists.");
            });

            studentGroup.MapPut("", async (IDynamoDBContext dbContext, Student request) =>
            {
                var student = await dbContext.LoadAsync<Student>(request.Id);
                if (student != null)
                {
                    await dbContext.SaveAsync<Student>(student);
                    return Results.Ok();
                }
                return Results.BadRequest("Student does not exists.");
            });

            studentGroup.MapDelete("/{studentId}", async (IDynamoDBContext dbContext, int studentId) =>
            {
                var studnet = await dbContext.LoadAsync<Student>(studentId);
                if (studnet != null)
                {
                    await dbContext.DeleteAsync(studnet);
                    return Results.Ok();
                }
                return Results.BadRequest("Student does not exists");
            });


        }
    }
}

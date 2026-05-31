using Amazon.DynamoDBv2;
using Amazon.Lambda.AspNetCoreServer;
using FluentValidation;
using KnowMe.API.Features.Games;
using KnowMe.API.Features.Users;
using KnowMe.API.Persistence;
using KnowMe.API.Persistence.Repositories;
using KnowMe.API.Shared.Middleware;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Structured JSON logging for Lambda (queryable in CloudWatch Logs Insights).
if (!builder.Environment.IsDevelopment())
{
    builder.Logging.AddJsonConsole(options => options.IncludeScopes = true);
}

// AWS Lambda hosting (HTTP API Gateway v2 proxy).
builder.Services.AddAWSLambdaHosting(LambdaEventSource.HttpApi);

// DynamoDB configuration.
builder.Services
    .AddOptions<DynamoDbSettings>()
    .Bind(builder.Configuration.GetSection(DynamoDbSettings.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddSingleton<IAmazonDynamoDB>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<DynamoDbSettings>>().Value;

    // ServiceUrl is only set for local development against DynamoDB Local.
    return string.IsNullOrEmpty(settings.ServiceUrl)
        ? new AmazonDynamoDBClient()
        : new AmazonDynamoDBClient(new AmazonDynamoDBConfig { ServiceURL = settings.ServiceUrl });
});

// CORS.
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader());
});

// Validation.
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Repositories.
builder.Services.AddScoped<IGameRepository, GameRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

var app = builder.Build();

// Middleware pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();
app.UseMiddleware<UserContextMiddleware>();

// Routes.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapUserEndpoints();
app.MapGameEndpoints();

app.Run();

// Exposed for the validator assembly scan and integration tests.
public partial class Program;

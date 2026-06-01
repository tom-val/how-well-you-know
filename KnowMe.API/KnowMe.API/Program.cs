using Amazon.ApiGatewayManagementApi;
using Amazon.DynamoDBv2;
using Amazon.Lambda.AspNetCoreServer;
using FluentValidation;
using KnowMe.API.Features.Games;
using KnowMe.API.Features.Suggestions;
using KnowMe.API.Features.Users;
using KnowMe.API.Persistence;
using KnowMe.API.Persistence.Notifications;
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

// Real-time notifications: broadcast game-changed over WebSockets when an endpoint is
// configured; otherwise a no-op (local dev / tests just rely on the frontend's safety poll).
builder.Services
    .AddOptions<WebSocketSettings>()
    .Bind(builder.Configuration.GetSection(WebSocketSettings.SectionName));

var wsEndpoint = builder.Configuration[$"{WebSocketSettings.SectionName}:ManagementEndpoint"];
if (string.IsNullOrEmpty(wsEndpoint))
{
    builder.Services.AddSingleton<IGameNotifier, NoOpGameNotifier>();
}
else
{
    builder.Services.AddSingleton<IAmazonApiGatewayManagementApi>(_ =>
        new AmazonApiGatewayManagementApiClient(
            new AmazonApiGatewayManagementApiConfig { ServiceURL = wsEndpoint }));
    builder.Services.AddSingleton<IGameNotifier, WebSocketGameNotifier>();
}

// AI question suggestions (inline OpenAI call; short timeout so a slow provider can't
// hold the request near the API Gateway cap — the client falls back to the static bank).
builder.Services
    .AddOptions<OpenAiSettings>()
    .Bind(builder.Configuration.GetSection(OpenAiSettings.SectionName));

builder.Services.AddHttpClient<IQuestionSuggester, OpenAiQuestionSuggester>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(12);
});

var app = builder.Build();

// Middleware pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseCors();

// Production trusts the verified user id from the Cognito Lambda authorizer; local
// development and tests fall back to the X-User-Id header so no Cognito is required.
if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Testing"))
{
    app.UseMiddleware<UserContextMiddleware>();
}
else
{
    app.UseMiddleware<AuthorizerContextMiddleware>();
}

// Routes.
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapUserEndpoints();
app.MapGameEndpoints();
app.MapSuggestionEndpoints();

app.Run();

// Exposed for the validator assembly scan and integration tests.
public partial class Program;

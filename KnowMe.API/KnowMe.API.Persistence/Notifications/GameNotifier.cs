using System.Text;
using System.Text.Json;
using Amazon.ApiGatewayManagementApi;
using Amazon.ApiGatewayManagementApi.Model;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KnowMe.API.Persistence.Notifications;

/// <summary>
/// Pushes lightweight "game-changed" signals to every WebSocket connection subscribed to a
/// game, so clients can invalidate their cache and refetch immediately instead of polling.
/// </summary>
public interface IGameNotifier
{
    Task GameChangedAsync(Guid gameId, CancellationToken cancellationToken = default);
}

/// <summary>No-op used when WebSocket broadcasting is not configured (local dev, tests).</summary>
public sealed class NoOpGameNotifier : IGameNotifier
{
    public Task GameChangedAsync(Guid gameId, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}

/// <summary>
/// Looks up live connections for a game via the connections table GSI and posts to each one
/// through the API Gateway Management API. Best-effort: failures are logged but never bubble
/// up, since a missed notification only delays the safety-net refetch — it can't corrupt state.
/// Stale connections (HTTP 410) are pruned from the table.
/// </summary>
public sealed class WebSocketGameNotifier : IGameNotifier
{
    private const string GameIdIndex = "game_id-index";
    private const string ConnectionIdAttribute = "connection_id";
    private const string GameIdAttribute = "game_id";

    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly IAmazonApiGatewayManagementApi _management;
    private readonly string _connectionsTableName;
    private readonly ILogger<WebSocketGameNotifier> _logger;

    public WebSocketGameNotifier(
        IAmazonDynamoDB dynamoDb,
        IAmazonApiGatewayManagementApi management,
        IOptions<DynamoDbSettings> settings,
        ILogger<WebSocketGameNotifier> logger)
    {
        _dynamoDb = dynamoDb;
        _management = management;
        _connectionsTableName = settings.Value.ConnectionsTableName;
        _logger = logger;
    }

    public async Task GameChangedAsync(Guid gameId, CancellationToken cancellationToken = default)
    {
        try
        {
            var connections = await _dynamoDb.QueryAsync(new QueryRequest
            {
                TableName = _connectionsTableName,
                IndexName = GameIdIndex,
                KeyConditionExpression = $"{GameIdAttribute} = :g",
                ExpressionAttributeValues = new Dictionary<string, AttributeValue>
                {
                    [":g"] = new() { S = gameId.ToString() }
                }
            }, cancellationToken);

            if (connections.Items.Count == 0)
            {
                return;
            }

            var payload = JsonSerializer.SerializeToUtf8Bytes(
                new { type = "game-changed", gameId = gameId.ToString() });

            var sends = connections.Items
                .Select(item => item[ConnectionIdAttribute].S)
                .Select(connectionId => PostAsync(connectionId, payload, cancellationToken));

            await Task.WhenAll(sends);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast game-changed for {GameId}.", gameId);
        }
    }

    private async Task PostAsync(string connectionId, byte[] payload, CancellationToken cancellationToken)
    {
        try
        {
            await _management.PostToConnectionAsync(new PostToConnectionRequest
            {
                ConnectionId = connectionId,
                Data = new MemoryStream(payload)
            }, cancellationToken);
        }
        catch (GoneException)
        {
            // Socket already closed — prune the stale row.
            await _dynamoDb.DeleteItemAsync(new DeleteItemRequest
            {
                TableName = _connectionsTableName,
                Key = new Dictionary<string, AttributeValue>
                {
                    [ConnectionIdAttribute] = new() { S = connectionId }
                }
            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to post to connection {ConnectionId}.", connectionId);
        }
    }
}

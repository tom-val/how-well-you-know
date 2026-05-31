using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using KnowMe.API.Domain.Entities;
using KnowMe.API.Persistence.Mapping;
using KnowMe.API.Persistence.Records;
using Microsoft.Extensions.Options;

namespace KnowMe.API.Persistence.Repositories;

public interface IGameRepository
{
    Task<Game?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the aggregate with optimistic concurrency. Throws
    /// <see cref="ConcurrencyException"/> if the stored version changed since it was loaded.
    /// </summary>
    Task SaveAsync(Game game, CancellationToken cancellationToken = default);
}

/// <summary>
/// Stores each Game aggregate as a single DynamoDB item: the whole graph serialised to a
/// JSON document attribute, guarded by a numeric version for optimistic locking. Scoped per
/// request so the version observed at load time is reused on save.
/// </summary>
public class GameRepository : IGameRepository
{
    private const string IdAttribute = "id";
    private const string VersionAttribute = "version";
    private const string DataAttribute = "data";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    private readonly Dictionary<Guid, int> _loadedVersions = new();

    public GameRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDbSettings> settings)
    {
        _dynamoDb = dynamoDb;
        _tableName = settings.Value.GamesTableName;
    }

    public async Task<Game?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue> { [IdAttribute] = new() { S = id.ToString() } },
            ConsistentRead = true
        }, cancellationToken);

        if (response.Item is null || response.Item.Count == 0)
        {
            return null;
        }

        var version = int.Parse(response.Item[VersionAttribute].N);
        _loadedVersions[id] = version;

        var record = JsonSerializer.Deserialize<GameRecord>(response.Item[DataAttribute].S, JsonOptions)
            ?? throw new InvalidOperationException($"Stored game {id} could not be deserialised.");

        return GameMapper.ToDomain(record);
    }

    public async Task SaveAsync(Game game, CancellationToken cancellationToken = default)
    {
        var record = GameMapper.ToRecord(game);
        var json = JsonSerializer.Serialize(record, JsonOptions);

        var isExisting = _loadedVersions.TryGetValue(game.Id, out var expectedVersion);
        var newVersion = isExisting ? expectedVersion + 1 : 1;

        var request = new PutItemRequest
        {
            TableName = _tableName,
            Item = new Dictionary<string, AttributeValue>
            {
                [IdAttribute] = new() { S = game.Id.ToString() },
                [VersionAttribute] = new() { N = newVersion.ToString() },
                [DataAttribute] = new() { S = json }
            }
        };

        if (isExisting)
        {
            request.ConditionExpression = $"{VersionAttribute} = :expected";
            request.ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":expected"] = new() { N = expectedVersion.ToString() }
            };
        }
        else
        {
            request.ConditionExpression = $"attribute_not_exists({IdAttribute})";
        }

        try
        {
            await _dynamoDb.PutItemAsync(request, cancellationToken);
        }
        catch (ConditionalCheckFailedException)
        {
            throw new ConcurrencyException(
                $"Game {game.Id} was modified by another request. Reload and retry.");
        }

        _loadedVersions[game.Id] = newVersion;
    }
}

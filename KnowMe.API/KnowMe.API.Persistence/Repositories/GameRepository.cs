using System.Text.Json;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using KnowMe.API.Domain.Entities;
using KnowMe.API.Persistence.Mapping;
using KnowMe.API.Persistence.Records;
using Microsoft.Extensions.Options;

namespace KnowMe.API.Persistence.Repositories;

/// <summary>
/// Denormalised summary of a game a user belongs to, read from the memberships index
/// so a player's games can be listed without scanning every game.
/// </summary>
public record GameMembership(
    Guid GameId,
    string Name,
    string Status,
    Guid CreatedByUser,
    DateTimeOffset CreatedAt,
    int QuestionCount,
    int PlayerCount);

public interface IGameRepository
{
    Task<Game?> GetAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the aggregate with optimistic concurrency, atomically refreshing the
    /// membership index for its players. Throws <see cref="ConcurrencyException"/> if the
    /// stored version changed since it was loaded.
    /// </summary>
    Task SaveAsync(Game game, CancellationToken cancellationToken = default);

    /// <summary>Lists every game the given user has created or joined.</summary>
    Task<IReadOnlyList<GameMembership>> ListForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Stores each Game aggregate as a single DynamoDB item: the whole graph serialised to a
/// JSON document attribute, guarded by a numeric version for optimistic locking. Scoped per
/// request so the version observed at load time is reused on save. A second "memberships"
/// table holds an inverted index (one row per player per game) maintained in the same
/// transaction as the aggregate, enabling "list my games".
/// </summary>
public class GameRepository : IGameRepository
{
    private const string IdAttribute = "id";
    private const string VersionAttribute = "version";
    private const string DataAttribute = "data";

    // Membership item attributes.
    private const string UserIdAttribute = "user_id";
    private const string GameIdAttribute = "game_id";
    private const string NameAttribute = "name";
    private const string StatusAttribute = "status";
    private const string CreatedByAttribute = "created_by_user";
    private const string CreatedAtAttribute = "created_at";
    private const string QuestionCountAttribute = "question_count";
    private const string PlayerCountAttribute = "player_count";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    private readonly string _membershipsTableName;
    private readonly Dictionary<Guid, int> _loadedVersions = new();

    public GameRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDbSettings> settings)
    {
        _dynamoDb = dynamoDb;
        _tableName = settings.Value.GamesTableName;
        _membershipsTableName = settings.Value.MembershipsTableName;
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

        var gamePut = new Put
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
            gamePut.ConditionExpression = $"{VersionAttribute} = :expected";
            gamePut.ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":expected"] = new() { N = expectedVersion.ToString() }
            };
        }
        else
        {
            gamePut.ConditionExpression = $"attribute_not_exists({IdAttribute})";
        }

        var transactItems = new List<TransactWriteItem> { new() { Put = gamePut } };

        // One membership row per current player, with the game summary denormalised onto it.
        transactItems.AddRange(game.Players.Select(player => new TransactWriteItem
        {
            Put = new Put
            {
                TableName = _membershipsTableName,
                Item = new Dictionary<string, AttributeValue>
                {
                    [UserIdAttribute] = new() { S = player.Id.ToString() },
                    [GameIdAttribute] = new() { S = game.Id.ToString() },
                    [NameAttribute] = new() { S = game.Name },
                    [StatusAttribute] = new() { S = game.Status.ToString() },
                    [CreatedByAttribute] = new() { S = game.CreatedByUser.ToString() },
                    [CreatedAtAttribute] = new() { S = game.CreatedAt.ToString("O") },
                    [QuestionCountAttribute] = new() { N = game.Questions.Count.ToString() },
                    [PlayerCountAttribute] = new() { N = game.Players.Count.ToString() }
                }
            }
        }));

        try
        {
            await _dynamoDb.TransactWriteItemsAsync(
                new TransactWriteItemsRequest { TransactItems = transactItems }, cancellationToken);
        }
        catch (TransactionCanceledException ex)
            when (ex.CancellationReasons.Any(r => r.Code == "ConditionalCheckFailed"))
        {
            throw new ConcurrencyException(
                $"Game {game.Id} was modified by another request. Reload and retry.");
        }

        _loadedVersions[game.Id] = newVersion;
    }

    public async Task<IReadOnlyList<GameMembership>> ListForUserAsync(
        Guid userId, CancellationToken cancellationToken = default)
    {
        var response = await _dynamoDb.QueryAsync(new QueryRequest
        {
            TableName = _membershipsTableName,
            KeyConditionExpression = $"{UserIdAttribute} = :userId",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":userId"] = new() { S = userId.ToString() }
            }
        }, cancellationToken);

        return response.Items
            .Select(item => new GameMembership(
                Guid.Parse(item[GameIdAttribute].S),
                item[NameAttribute].S,
                item[StatusAttribute].S,
                Guid.Parse(item[CreatedByAttribute].S),
                DateTimeOffset.Parse(item[CreatedAtAttribute].S),
                item.TryGetValue(QuestionCountAttribute, out var qc) ? int.Parse(qc.N) : 0,
                item.TryGetValue(PlayerCountAttribute, out var pc) ? int.Parse(pc.N) : 0))
            .ToList();
    }
}

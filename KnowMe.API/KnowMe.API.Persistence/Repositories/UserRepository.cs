using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using KnowMe.API.Domain.Entities;
using KnowMe.API.Persistence.Mapping;
using KnowMe.API.Persistence.Records;
using Microsoft.Extensions.Options;

namespace KnowMe.API.Persistence.Repositories;

public interface IUserRepository
{
    Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    Task SaveAsync(User user, CancellationToken cancellationToken = default);
}

public class UserRepository : IUserRepository
{
    private const string IdAttribute = "id";
    private const string UserNameAttribute = "user_name";
    private const string ProfileUrlAttribute = "profile_url";
    private const string CreatedAtAttribute = "created_at";

    private readonly IAmazonDynamoDB _dynamoDb;
    private readonly string _tableName;
    private readonly string _userNameIndexName;

    public UserRepository(IAmazonDynamoDB dynamoDb, IOptions<DynamoDbSettings> settings)
    {
        _dynamoDb = dynamoDb;
        _tableName = settings.Value.UsersTableName;
        _userNameIndexName = settings.Value.UsersByUserNameIndexName;
    }

    public async Task<User?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var response = await _dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = _tableName,
            Key = new Dictionary<string, AttributeValue> { [IdAttribute] = new() { S = id.ToString() } }
        }, cancellationToken);

        return response.Item is null || response.Item.Count == 0 ? null : ToDomain(response.Item);
    }

    public async Task<User?> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
    {
        var response = await _dynamoDb.QueryAsync(new QueryRequest
        {
            TableName = _tableName,
            IndexName = _userNameIndexName,
            KeyConditionExpression = $"{UserNameAttribute} = :name",
            ExpressionAttributeValues = new Dictionary<string, AttributeValue>
            {
                [":name"] = new() { S = userName }
            },
            Limit = 1
        }, cancellationToken);

        return response.Items is { Count: > 0 } ? ToDomain(response.Items[0]) : null;
    }

    public async Task SaveAsync(User user, CancellationToken cancellationToken = default)
    {
        var record = UserMapper.ToRecord(user);

        var item = new Dictionary<string, AttributeValue>
        {
            [IdAttribute] = new() { S = record.Id.ToString() },
            [UserNameAttribute] = new() { S = record.UserName },
            [CreatedAtAttribute] = new() { S = record.CreatedAt.ToString("O") }
        };

        if (!string.IsNullOrEmpty(record.ProfileUrl))
        {
            item[ProfileUrlAttribute] = new AttributeValue { S = record.ProfileUrl };
        }

        await _dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = _tableName,
            Item = item
        }, cancellationToken);
    }

    private static User ToDomain(Dictionary<string, AttributeValue> item)
    {
        var profileUrl = item.TryGetValue(ProfileUrlAttribute, out var profile) ? profile.S : null;

        return UserMapper.ToDomain(new UserRecord
        {
            Id = Guid.Parse(item[IdAttribute].S),
            UserName = item[UserNameAttribute].S,
            ProfileUrl = profileUrl,
            CreatedAt = DateTimeOffset.Parse(item[CreatedAtAttribute].S)
        });
    }
}

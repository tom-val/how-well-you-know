using System.ComponentModel.DataAnnotations;

namespace KnowMe.API.Persistence;

public class DynamoDbSettings
{
    public const string SectionName = "DynamoDb";

    [Required]
    public string GamesTableName { get; set; } = "knowme-games";

    [Required]
    public string UsersTableName { get; set; } = "knowme-users";

    /// <summary>
    /// Name of the global secondary index on the users table that maps username -> user.
    /// </summary>
    [Required]
    public string UsersByUserNameIndexName { get; set; } = "user_name-index";

    /// <summary>
    /// Optional service URL override for pointing at DynamoDB Local during development.
    /// </summary>
    public string? ServiceUrl { get; set; }
}

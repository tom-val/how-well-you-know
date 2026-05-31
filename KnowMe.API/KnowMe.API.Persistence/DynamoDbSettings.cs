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
    /// Inverted index of game memberships keyed by user, so a player's games can be listed.
    /// </summary>
    [Required]
    public string MembershipsTableName { get; set; } = "knowme-memberships";

    /// <summary>
    /// Optional service URL override for pointing at DynamoDB Local during development.
    /// </summary>
    public string? ServiceUrl { get; set; }
}

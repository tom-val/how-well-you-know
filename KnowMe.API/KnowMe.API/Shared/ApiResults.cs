using KnowMe.API.Domain.Validation;

namespace KnowMe.API.Shared;

public static class ApiResults
{
    /// <summary>
    /// Renders domain validation failures as an RFC 7807 validation problem (HTTP 400).
    /// </summary>
    public static IResult DomainValidationProblem(IEnumerable<ValidationError> errors) =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["game"] = errors.Select(e => e.Message).ToArray()
        });
}

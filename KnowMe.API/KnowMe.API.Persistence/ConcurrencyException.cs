namespace KnowMe.API.Persistence;

/// <summary>
/// Thrown when an optimistic-concurrency conditional write fails because the
/// stored aggregate was modified by another request since it was loaded.
/// </summary>
public class ConcurrencyException : Exception
{
    public ConcurrencyException(string message) : base(message) { }
}

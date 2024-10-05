namespace KnowMe.API.Domain.Validation;

public record Result<T>
{
    private readonly T? _value;

    public T Value =>
        IsSuccess ? _value! : throw new InvalidOperationException("Failure result value cannot be accessed.");

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public List<ValidationError>? Errors { get; private set; }

    private Result(bool isSuccess, T? val, List<ValidationError>? errors)
    {
        IsSuccess = isSuccess;
        _value = val;
        Errors = errors;
    }

    public static Result<TValue> Success<TValue>(TValue value) => new(true, value, null);

    public static Result<T> Failure(List<ValidationError> errors) => new(false, default, errors);
}

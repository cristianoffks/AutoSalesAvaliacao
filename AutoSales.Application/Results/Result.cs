namespace AutoSales.Application.Results;

public sealed record Result<T>(bool IsSuccess, T? Value, string? Message, string? Field, ErrorType ErrorType = ErrorType.None)
{
    public static Result<T> Success(T value)
    {
        return new(true, value, null, null);
    }

    public static Result<T> Failure(string message, string? field = null, ErrorType errorType = ErrorType.Conflict)
    {
        return new(false, default, message, field, errorType);
    }
}

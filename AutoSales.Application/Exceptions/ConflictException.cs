namespace AutoSales.Application.Exceptions;

public sealed class ConflictException(string message, Exception? innerException = null, string? field = null)
    : Exception(message, innerException)
{
    public string? Field { get; } = field ?? (innerException as ConflictException)?.Field;
}

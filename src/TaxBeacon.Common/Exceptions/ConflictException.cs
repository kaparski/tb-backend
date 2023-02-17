namespace TaxBeacon.Common.Exceptions;

public class ConflictException: Exception
{
    public string Key { get; }

    public ConflictException(string? message, string key) : base(message) => Key = key;
}

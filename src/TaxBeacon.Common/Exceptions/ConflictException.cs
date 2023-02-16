namespace TaxBeacon.Common.Exceptions;

public class ConflictException: Exception
{
    public string Message { get; }
    public string Key { get; }

    public ConflictException(string message, string key)
    {
        Message = message;
        Key = key;
    }
}

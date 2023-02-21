using TaxBeacon.Common.Enums;

namespace TaxBeacon.Common.Exceptions;

public class ConflictException: Exception
{
    public ConflictExceptionKey Key { get; }

    public ConflictException(string? message, ConflictExceptionKey key) : base(message) => Key = key;
}

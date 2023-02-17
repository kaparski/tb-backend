namespace TaxBeacon.Common.Enums;

public enum ConflictExceptionKey
{
    UserEmail = 1
}

public static class ConflictExceptionMessages
{
    public const string EmailExistsMessage = "User with the same email already exists";
}

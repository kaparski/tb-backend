using TaxBeacon.Common.Enums;

namespace TaxBeacon.Common.Constants;

public static class ExceptionMessages
{
    public static readonly Dictionary<ExceptionKey, string> Messages = new()
    {
        [ExceptionKey.EmailExists] = "User with the same email already exists"
    };
}

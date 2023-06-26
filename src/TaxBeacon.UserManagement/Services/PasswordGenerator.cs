using System.Text;

namespace TaxBeacon.UserManagement.Services;

public sealed class PasswordGenerator: IPasswordGenerator
{
    private static readonly Random _random = new();

    public string GeneratePassword()
    {
        const string allowedSymbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        var length = 8;
        var res = new StringBuilder();
        while (0 < length--)
        {
            res.Append(allowedSymbols[_random.Next(allowedSymbols.Length)]);
        }
        return res.ToString();
    }
}

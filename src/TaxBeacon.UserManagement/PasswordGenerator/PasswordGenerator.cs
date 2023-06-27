using System.Text;

namespace TaxBeacon.UserManagement.PasswordGenerator;

public sealed class PasswordGenerator: IPasswordGenerator
{
    private static readonly Random Random = new();

    public string GeneratePassword()
    {
        const string allowedSymbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
        var length = 8;
        var res = new StringBuilder();
        while (0 < length--)
        {
            res.Append(allowedSymbols[Random.Next(allowedSymbols.Length)]);
        }
        return res.ToString();
    }
}

using System.Text.RegularExpressions;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Extensions;
using TaxBeacon.DAL.Accounts.Common;

namespace TaxBeacon.Accounts.Common;

public static partial class Helpers
{
    [GeneratedRegex(@"(?<Type>Office|Mobile|Fax): (?<Number>\(\d{3}\) \d{3}-\d{4}|\d{1,15})(, (?<Extension>.*))?", RegexOptions.IgnoreCase)]
    private static partial Regex PhoneStringRegex();

    public static IEnumerable<T> GetPhonesFromString<T>(string? source) where T: PhoneBase
    {
        if (string.IsNullOrEmpty(source)) return Enumerable.Empty<T>();

        var phones = new List<T>();

        foreach (var line in source.Split(Environment.NewLine))
        {
            var match = PhoneStringRegex().Match(line);

            if (!match.Success) continue;

            var phone = Activator.CreateInstance<T>();
            phone.Type = Enum.Parse<PhoneType>(match.Groups["Type"].Value);
            phone.Number = match.Groups["Number"].Value.RemovePhoneMask()!;
            phone.Extension = match.Groups["Extension"].Value;

            phones.Add(phone);
        }

        return phones;
    }
}

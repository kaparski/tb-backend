using System.Text;
using System.Text.RegularExpressions;

namespace TaxBeacon.Common.Extensions;

public static partial class StringExtensions
{
    [GeneratedRegex(@"^(\d{3})(\d{3})(\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex UsPhoneMaskRegex();

    [GeneratedRegex(@"^(\d{5})(\d{4})?$", RegexOptions.IgnoreCase)]
    private static partial Regex UsZipMaskRegex();

    [GeneratedRegex(@"^(\d{2})(\d{7})?$", RegexOptions.IgnoreCase)]
    private static partial Regex FeinMaskRegex();

    // (123) 456-7899
    [GeneratedRegex(@"^\((\d{3})\) (\d{3})-(\d{4})", RegexOptions.IgnoreCase)]
    private static partial Regex UsPhoneRegex();

    // 12345
    // 12345-6789
    [GeneratedRegex(@"^(\d{5})-(\d{4})?$", RegexOptions.IgnoreCase)]
    private static partial Regex UsZipRegex();

    // 12-3456789
    [GeneratedRegex(@"^(\d{2})-(\d{7})?$", RegexOptions.IgnoreCase)]
    private static partial Regex FeinRegex();

    private static readonly HashSet<char> SpecialCharacters = new("+-&|!(){}[]^\"~*?:\\/");

    public static string? ApplyPhoneMask(this string? number)
    {
        if (string.IsNullOrEmpty(number))
        {
            return number;
        }

        var match = UsPhoneMaskRegex().Match(number);

        return match.Groups.Count == 4 ? $"({match.Groups[1]}) {match.Groups[2]}-{match.Groups[3]}" : number;
    }

    public static string? RemovePhoneMask(this string? number)
    {
        if (string.IsNullOrEmpty(number))
        {
            return number;
        }

        var match = UsPhoneRegex().Match(number);

        return match.Groups.Count != 4
            ? number
            : $"{match.Groups[1]}{match.Groups[2]}{match.Groups[3]}";
    }

    public static string? ApplyZipMask(this string? zip)
    {
        if (string.IsNullOrEmpty(zip))
        {
            return zip;
        }

        var match = UsZipMaskRegex().Match(zip);

        return string.IsNullOrEmpty(match.Groups[1].Value)
            ? zip
            : string.IsNullOrEmpty(match.Groups[2].Value)
                ? match.Groups[1].Value
                : $"{match.Groups[1]}-{match.Groups[2]}";
    }

    public static string? RemoveZipMask(this string? zip)
    {
        if (string.IsNullOrEmpty(zip))
        {
            return zip;
        }

        var match = UsZipRegex().Match(zip);

        return string.IsNullOrEmpty(match.Groups[1].Value)
            ? zip
            : $"{match.Groups[1]}{match.Groups[2]}";
    }

    public static string? ApplyFeinMask(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var match = FeinMaskRegex().Match(value);

        return match.Groups.Count switch
        {
            2 => match.Groups[1].Value,
            3 => $"{match.Groups[1]}-{match.Groups[2]}",
            _ => value
        };
    }

    public static string? RemoveFeinMask(this string? fein)
    {
        if (string.IsNullOrEmpty(fein))
        {
            return fein;
        }

        var match = FeinRegex().Match(fein);

        return match.Groups.Count switch
        {
            2 => match.Groups[1].Value,
            3 => $"{match.Groups[1]}{match.Groups[2]}",
            _ => fein
        };
    }

    public static string? EscapeSpecialCharacters(this string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return value;
        }

        var builder = new StringBuilder();
        var left = 0;
        for (var right = 0; right < value.Length; right++)
        {
            if (!SpecialCharacters.Contains(value[right]))
            {
                continue;
            }

            builder.Append(value.AsSpan(left, right - left));
            builder.Append(@"\");
            left = right;
        }

        builder.Append(value.AsSpan(left, value.Length - left));

        return builder.ToString();
    }
}

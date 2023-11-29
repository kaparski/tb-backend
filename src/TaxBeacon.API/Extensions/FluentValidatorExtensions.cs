using FluentValidation;
using System.Text.RegularExpressions;

namespace TaxBeacon.Common.Extensions;
public static partial class FluentValidatorExtensions
{
    private static readonly Regex WebLinkRegex = LinkRegex();

    /// <summary>
    ///
    /// <para>Validates if web link starts with https and matches correct URL format.</para>
    /// <para>Returns "Invalid URL format. See example:"https://example.com"" validation message.</para>
    /// </summary>
    /// <returns>Returns builder for chaining</returns>
    ///
    public static IRuleBuilderOptions<T, string?> WebLink<T>(this IRuleBuilder<T, string?> ruleBuilder, int maxLength = 4000)
        => ruleBuilder
        .MaximumLength(maxLength)
        .Matches(WebLinkRegex)
            .WithMessage("Invalid URL format. See example:\"https://example.com\"");
    ///
    [GeneratedRegex("^(https:\\/\\/)[\\w.-]+(?:\\.[\\w\\.-]+)+[\\w\\-\\._~:/?#[\\]@!\\$&'\\(\\)\\*\\+,;=.]+$")]
    private static partial Regex LinkRegex();
}

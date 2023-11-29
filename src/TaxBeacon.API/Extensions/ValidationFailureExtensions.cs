using FluentValidation.Results;

namespace TaxBeacon.API.Extensions;

public static class ValidationFailureExtensions
{
    public static List<ValidationFailure> ConvertPropertyNameToCamelCase(this List<ValidationFailure> failures)
    {
        foreach (var validationFailure in failures)
        {
            validationFailure.PropertyName = string.Join(".", validationFailure
                .PropertyName
                .Split('.')
                .Select(part => System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(part)));
        }

        return failures;
    }
}

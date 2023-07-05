using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Divisions.Requests;

public record ExportDivisionsRequest(FileType FileType, string IanaTimeZone);

public class ExportTenantDivisionsRequestValidator: AbstractValidator<ExportDivisionsRequest>
{
    public ExportTenantDivisionsRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}
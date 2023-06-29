using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Tenants.Requests;

public record ExportTenantsRequest(FileType FileType, string IanaTimeZone);

public class ExportTenantsRequestValidator: AbstractValidator<ExportTenantsRequest>
{
    public ExportTenantsRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}
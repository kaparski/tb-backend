using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Locations.Requests;

public record ExportLocationsRequest(FileType FileType, string IanaTimeZone);

public class ExportLocationsRequestValidator: AbstractValidator<ExportLocationsRequest>
{
    public ExportLocationsRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

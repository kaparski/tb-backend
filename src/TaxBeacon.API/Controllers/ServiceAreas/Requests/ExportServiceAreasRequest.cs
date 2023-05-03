using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.ServiceAreas.Requests;

public record ExportServiceAreasRequest(FileType FileType, string IanaTimeZone);

public class ExportServiceAreasRequestValidator: AbstractValidator<ExportServiceAreasRequest>
{
    public ExportServiceAreasRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

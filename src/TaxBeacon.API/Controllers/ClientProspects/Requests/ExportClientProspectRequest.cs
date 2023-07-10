using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.ClientProspects.Requests;

public record ExportClientProspectRequest(FileType FileType, string IanaTimeZone);

public class ExportClientProspectRequestValidator: AbstractValidator<ExportClientProspectRequest>
{
    public ExportClientProspectRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

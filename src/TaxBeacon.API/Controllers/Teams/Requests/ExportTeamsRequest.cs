using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Teams.Requests;

public record ExportTeamsRequest(FileType FileType, string IanaTimeZone);

public class ExportTeamsRequestValidator: AbstractValidator<ExportTeamsRequest>
{
    public ExportTeamsRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

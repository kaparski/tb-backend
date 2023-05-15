using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Programs.Requests;

public record ExportProgramsRequest(FileType FileType, string IanaTimeZone);

public class ExportProgramsRequestValidator: AbstractValidator<ExportProgramsRequest>
{
    public ExportProgramsRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}
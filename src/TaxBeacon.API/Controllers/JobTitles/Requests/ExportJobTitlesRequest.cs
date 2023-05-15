using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.JobTitles.Requests;

public record ExportJobTitlesRequest(FileType FileType, string IanaTimeZone);

public class ExportJobTitlesRequestValidator: AbstractValidator<ExportJobTitlesRequest>
{
    public ExportJobTitlesRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

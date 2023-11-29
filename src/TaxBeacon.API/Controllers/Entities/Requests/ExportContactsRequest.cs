using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Entities.Requests;

public record ExportAccountEntitiesRequest(FileType FileType, string IanaTimeZone);

public class ExportAccountEntitiesRequestValidator: AbstractValidator<ExportAccountEntitiesRequest>
{
    public ExportAccountEntitiesRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

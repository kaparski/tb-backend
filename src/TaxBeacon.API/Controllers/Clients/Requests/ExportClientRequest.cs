using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Clients.Requests;

public record ExportClientRequest(FileType FileType, string IanaTimeZone);

public class ExportClientRequestValidator: AbstractValidator<ExportClientRequest>
{
    public ExportClientRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Contacts.Requests;

public record ExportContactsRequest(FileType FileType, string IanaTimeZone);

public class ExportContactsRequestValidator: AbstractValidator<ExportContactsRequest>
{
    public ExportContactsRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Documents.Requests;

public record ExportDocumentsRequest(FileType FileType, string IanaTimeZone);

public class ExportDocumentsRequestValidator: AbstractValidator<ExportDocumentsRequest>
{
    public ExportDocumentsRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

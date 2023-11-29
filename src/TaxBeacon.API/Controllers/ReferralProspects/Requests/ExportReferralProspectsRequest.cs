using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.ReferralProspects.Requests;

public record ExportReferralProspectsRequest(FileType FileType, string IanaTimeZone);

public class ExportRederralProspectsRequestValidator: AbstractValidator<ExportReferralProspectsRequest>
{
    public ExportRederralProspectsRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

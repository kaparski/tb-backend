using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.ReferralPartners.Requests;

public record ExportReferralPartnersRequest(FileType FileType, string IanaTimeZone);

public class ExportReferralPartnersRequestValidator: AbstractValidator<ExportReferralPartnersRequest>
{
    public ExportReferralPartnersRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

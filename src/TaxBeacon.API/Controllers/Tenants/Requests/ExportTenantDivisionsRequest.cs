using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Tenants.Requests
{
    public record ExportTenantDivisionsRequest(FileType FileType, string IanaTimeZone);

    public class ExportTenantDivisionsRequestValidator: AbstractValidator<ExportTenantDivisionsRequest>
    {
        public ExportTenantDivisionsRequestValidator()
        {
            RuleFor(x => x.FileType)
                .IsInEnum();

            RuleFor(x => x.IanaTimeZone)
                .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
        }
    }
}

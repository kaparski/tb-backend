using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Users.Requests
{
    public record ExportTenantsRequest(FileType FileType, string IanaTimeZone);

    public class ExportTenantsRequestValidator: AbstractValidator<ExportUsersRequest>
    {
        public ExportTenantsRequestValidator()
        {
            RuleFor(x => x.FileType)
                .IsInEnum();

            RuleFor(x => x.IanaTimeZone)
                .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
        }
    }
}

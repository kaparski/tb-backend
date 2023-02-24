using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Users.Requests
{
    public record ExportUsersRequest(FileType FileType, string IanaTimeZone);

    public class ExportUsersRequestValidator: AbstractValidator<ExportUsersRequest>
    {
        public ExportUsersRequestValidator()
        {
            RuleFor(x => x.FileType)
                .IsInEnum();

            RuleFor(x => x.IanaTimeZone)
                .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
        }
    }
}

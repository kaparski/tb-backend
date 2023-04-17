using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Users.Requests
{
    public record ExportDepartmentsRequest(FileType FileType, string IanaTimeZone);

    public class ExportDepartmentsRequestValidator: AbstractValidator<ExportDepartmentsRequest>
    {
        public ExportDepartmentsRequestValidator()
        {
            RuleFor(x => x.FileType)
                .IsInEnum();

            RuleFor(x => x.IanaTimeZone)
                .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
        }
    }
}

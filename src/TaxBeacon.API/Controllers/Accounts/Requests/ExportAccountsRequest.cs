using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Accounts.Requests;

public record ExportAccountsRequest(FileType FileType, string IanaTimeZone);

public class ExportAccountsRequestValidator: AbstractValidator<ExportAccountsRequest>
{
    public ExportAccountsRequestValidator()
    {
        RuleFor(x => x.FileType)
            .IsInEnum();

        RuleFor(x => x.IanaTimeZone)
            .Must(x => TimeZoneConverter.TZConvert.KnownIanaTimeZoneNames.Contains(x));
    }
}

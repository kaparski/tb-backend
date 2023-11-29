using FluentValidation;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Templates.Requests;

public record GetImportTemplatesRequest
{
    public TemplateType TemplateType { get; init; }
}

public class GetImportTemplatesRequestValidator: AbstractValidator<GetImportTemplatesRequest>
{
    public GetImportTemplatesRequestValidator() =>
        RuleFor(x => x.TemplateType)
            .IsInEnum()
            .NotEqual(TemplateType.None);
}

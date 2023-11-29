using FileSignatures;
using FluentValidation;
using TaxBeacon.API.Shared.Validators;

namespace TaxBeacon.API.Controllers.Entities.Requests;

public record ImportEntitiesRequest(IFormFile File);

public class ImportEntitiesRequestValidator: AbstractValidator<ImportEntitiesRequest>
{
    public ImportEntitiesRequestValidator(IConfiguration config, IFileFormatInspector inspector)
    {
        var maxContentLength = config.GetValue<long?>("EntitiesImport:MaxContentLength");
        var allowedMimeTypes = config.GetValue<string[]>("EntitiesImport:AllowedMimeTypes");

        if (maxContentLength is null)
            throw new ArgumentNullException(nameof(maxContentLength));

        if (allowedMimeTypes is null || allowedMimeTypes.Any())
            throw new ArgumentNullException(nameof(allowedMimeTypes));

        RuleFor(x => x.File)
            .SetValidator(new FormFileValidator(inspector, maxContentLength.Value, allowedMimeTypes.ToHashSet()));
    }
}

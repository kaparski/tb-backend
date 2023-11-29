using FileSignatures;
using FluentValidation;

namespace TaxBeacon.API.Shared.Validators;

public class FormFileValidator: AbstractValidator<IFormFile>
{
    public FormFileValidator(IFileFormatInspector inspector, long maxContentLength, HashSet<string> allowedMimeTypes)
    {
        RuleFor(x => x.Length)
            .NotNull()
            .LessThanOrEqualTo(maxContentLength)
            .WithMessage("File size is larger than allowed");

        RuleFor(x => x.ContentType)
            .NotNull()
            .Must(allowedMimeTypes.Contains)
            .WithMessage("File type is not allowed");

        RuleFor(x => x)
            .Must(x =>
            {
                using var stream = x.OpenReadStream();
                var fileFormatMediaType = inspector.DetermineFileFormat(stream)?.MediaType;
                return fileFormatMediaType is not null && allowedMimeTypes.Contains(fileFormatMediaType);
            });
    }
}

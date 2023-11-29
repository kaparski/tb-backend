using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.DocumentManagement.BlobStorage.Models;

namespace TaxBeacon.DocumentManagement.Templates;

public interface ITemplatesService
{
    Task<OneOf<DownloadFileResultDto, NotFound>> GetTemplateAsync(TemplateType templateType,
        CancellationToken cancellationToken = default);
}

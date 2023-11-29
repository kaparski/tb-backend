using OneOf.Types;
using OneOf;
using TaxBeacon.Accounts.Documents.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Documents;
public interface IDocumentService
{
    OneOf<IQueryable<AccountDocumentDto>, NotFound> QueryDocuments(Guid accountId);

    Task<OneOf<byte[], NotFound>> ExportDocumentsAsync(Guid accountId,
    FileType fileType,
    CancellationToken cancellationToken = default);
}

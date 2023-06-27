using TaxBeacon.Accounts.Services.Contacts.Models;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Services.Contacts;

public interface IContactService
{
    IQueryable<ContactDto> QueryContacts();

    Task<OneOf<ContactDetailsDto, NotFound>> GetContactDetailsAsync(Guid contactId, Guid accountId, CancellationToken cancellationToken);

    Task<OneOf<ContactDetailsDto, NotFound>> UpdateContactStatusAsync(Guid contactId, Guid accountId,
            Status status,
            CancellationToken cancellationToken = default);

    Task<byte[]> ExportContactsAsync(FileType fileType,
            CancellationToken cancellationToken = default);
}

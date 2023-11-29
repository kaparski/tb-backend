using OneOf;
using OneOf.Types;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Contacts;

public interface IContactService
{
    IQueryable<ContactDto> QueryContacts();

    IQueryable<AccountContactDto> QueryAccountContacts(Guid accountId);

    Task<OneOf<ContactDto, NotFound>> GetContactDetailsAsync(Guid contactId,
        CancellationToken cancellationToken = default);

    Task<OneOf<AccountContactDto, NotFound>> GetAccountContactDetailsAsync(Guid accountId,
        Guid contactId,
        CancellationToken cancellationToken = default);

    Task<OneOf<AccountContactDto, NotFound>> UpdateAccountContactStatusAsync(Guid accountId,
        Guid contactId,
        Status status,
        CancellationToken cancellationToken = default);

    Task<OneOf<AccountContactDto, NotFound>> CreateNewContactAsync(Guid accountId,
        CreateContactDto createContactDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound, Conflict>> AssignContactToAccountAsync(Guid accountId,
        Guid contactId,
        ContactType contactType,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportContactsAsync(FileType fileType,
        CancellationToken cancellationToken = default);

    Task<OneOf<byte[], NotFound>> ExportAccountContactsAsync(Guid accountId,
        FileType fileType,
        CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetContactActivitiesAsync(Guid contactId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetAccountContactActivitiesAsync(Guid accountId,
        Guid contactId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound, InvalidOperation>> UnassociateContactWithAccount(Guid accountId,
        Guid contactId,
        CancellationToken cancellation);

    Task<OneOf<AccountContactDto, NotFound>> UpdateAccountContactAsync(Guid contactId,
        Guid accountId,
        UpdateAccountContactDto updateContact,
        CancellationToken cancellationToken = default);

    Task<OneOf<ContactDto, NotFound>> UpdateContactAsync(Guid contactId,
        UpdateContactDto updateContact,
        CancellationToken cancellationToken = default);

    // TODO: create a separate service for linked contact
    Task<OneOf<IQueryable<LinkedContactDetailsDto>, NotFound>> QueryLinkedContacts(Guid contactId,
        CancellationToken cancellationToken = default);

    Task<OneOf<Success, NotFound>> UnlinkContactFromContactAsync(Guid sourceContactId,
        Guid relatedContactIdId,
        CancellationToken cancellation = default);

    Task<OneOf<Success, NotFound, Conflict>> LinkContactToContactAsync(Guid sourceContactId,
        Guid relatedContactId,
        string comment,
        CancellationToken cancellationToken = default);
}

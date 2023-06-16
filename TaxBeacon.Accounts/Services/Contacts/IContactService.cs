using TaxBeacon.Accounts.Services.Contacts.Models;
using TaxBeacon.Common.Errors;
using OneOf;
using OneOf.Types;

namespace TaxBeacon.Accounts.Services.Contacts;

public interface IContactService
{
    Task<OneOf<Success<IQueryable<ContactDto>>, NotFound>> QueryContactsAsync(Guid accountId);

    Task<OneOf<ContactDetailsDto, NotFound>> GetContactDetailsAsync(Guid accountId, Guid contactId);
}

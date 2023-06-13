using TaxBeacon.Accounts.Services.Contacts.Models;

namespace TaxBeacon.Accounts.Services.Contacts;

public interface IContactService
{
    IQueryable<ContactDto> QueryContacts(Guid accountId);
}

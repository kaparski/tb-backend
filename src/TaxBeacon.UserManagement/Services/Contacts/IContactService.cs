using TaxBeacon.UserManagement.Services.Contacts.Models;

namespace TaxBeacon.UserManagement.Services.Contacts;

public interface IContactService
{
    public IQueryable<ContactDto> QueryContacts();
}

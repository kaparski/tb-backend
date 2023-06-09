using TaxBeacon.UserManagement.Services.Contacts.Models;

namespace TaxBeacon.UserManagement.Services.Contacts;

public class ContactService: IContactService
{
    public Task<IQueryable<ContactDto>> GetContactAsync(Guid id) => throw new NotImplementedException();
}

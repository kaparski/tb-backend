using Microsoft.Extensions.Logging;
using TaxBeacon.Accounts.Services.Contacts.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.Accounts.Services.Contacts;

public class ContactService: IContactService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountDbContext _context;

    public ContactService(ICurrentUserService currentUserService, IAccountDbContext context)
    {
        _currentUserService = currentUserService;
        _context = context;
    }

    public IQueryable<ContactDto> QueryContacts(Guid accountId)
    {
        var currentTenantId = _currentUserService.TenantId;
        var contacts = _context
            .Contacts
            .Where(x => x.TenantId == currentTenantId && x.AccountId == accountId)
            .Select(d => new ContactDto
            {
                Id = d.Id,
                FirstName = d.FirstName,
                LastName = d.LastName,
                FullName = d.FullName,
                Email = d.Email,
                JobTitle = d.JobTitle,
                ContactType = d.Type,
                Phone = d.Phone,
                Status = d.Status,
                Country = d.Country,
                City = d.City,
                State = d.State
            });

        return contacts;
    }
}

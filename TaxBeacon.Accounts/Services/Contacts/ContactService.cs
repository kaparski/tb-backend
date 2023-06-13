using Microsoft.Extensions.Logging;
using TaxBeacon.Accounts.Services.Contacts.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.Accounts.Services.Contacts;

public class ContactService: IContactService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ITaxBeaconDbContext _context;

    public ContactService(ICurrentUserService currentUserService, ITaxBeaconDbContext context)
    {
        _currentUserService = currentUserService;
        _context = context;
    }

    public IQueryable<ContactDto> QueryContacts()
    {
        var currentTenantId = _currentUserService.TenantId;
        var contacts = _context
            .Contacts
            .Where(x => x.TenantId == currentTenantId)
            .Select(d => new ContactDto
            {
                Id = d.Id,
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

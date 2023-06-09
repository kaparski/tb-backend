using Microsoft.Extensions.Logging;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Services.Contacts.Models;

namespace TaxBeacon.UserManagement.Services.Contacts;

public class ContactService: IContactService
{
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<ContactService> _logger;
    private readonly ITaxBeaconDbContext _context;

    public ContactService(ILogger<ContactService> logger, ICurrentUserService currentUserService, ITaxBeaconDbContext context)
    {
        _currentUserService = currentUserService;
        _context = context;
        _logger = logger;

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
                Email = d.Email.Address,
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

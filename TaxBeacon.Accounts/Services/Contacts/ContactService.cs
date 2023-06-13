using Microsoft.EntityFrameworkCore;
using TaxBeacon.Accounts.Services.Contacts.Models;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;
using OneOf;
using OneOf.Types;

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

    public async Task<OneOf<Success<IQueryable<ContactDto>>, NotFound>> QueryContactsAsync(Guid accountId)
    {
        var currentTenantId = _currentUserService.TenantId;
        var accountExists  = await _context.Accounts.AnyAsync(x => x.Id == accountId && x.TenantId == currentTenantId);
        if (!accountExists)
        {
            return new NotFound();
        }

        var contacts = _context
            .Contacts
            .Where(x => x.AccountId == accountId)
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
                State = d.State,
                AccountId = d.AccountId
            });

        return new Success<IQueryable<ContactDto>>(contacts);
    }
}

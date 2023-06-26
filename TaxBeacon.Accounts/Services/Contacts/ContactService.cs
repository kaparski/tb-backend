using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using System.Text.Json;
using TaxBeacon.Accounts.Services.Contacts.Models;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.DAL.Entities.Accounts;
using Microsoft.Extensions.Logging;
using Mapster;

namespace TaxBeacon.Accounts.Services.Contacts;

public class ContactService: IContactService
{
    private readonly ILogger<ContactService> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountDbContext _context;

    public ContactService(ILogger<ContactService> logger,
        ICurrentUserService currentUserService,
        IAccountDbContext context,
        IDateTimeService dateTimeService)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _context = context;
    }

    public async Task<OneOf<Success<IQueryable<ContactDto>>, NotFound>> QueryContactsAsync(Guid accountId)
    {
        var currentTenantId = _currentUserService.TenantId;
        var accountExists = await _context.Accounts.AnyAsync(x => x.Id == accountId && x.TenantId == currentTenantId);
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

    public async Task<OneOf<ContactDetailsDto, NotFound>> GetContactDetailsAsync(Guid contactId, Guid accountId, CancellationToken cancellationToken)
    {
        var currentTenantId = _currentUserService.TenantId;

        var contactDetailsDto = await _context
            .Contacts
            .Where(x => x.AccountId == accountId && x.Id == contactId && x.TenantId == currentTenantId)
            .Select(x => new ContactDetailsDto()
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                FullName = x.FullName,
                Email = x.Email,
                JobTitle = x.JobTitle,
                Type = x.Type,
                Phone = x.Phone,
                Phone2 = x.Phone2,
                Status = x.Status,
                Country = x.Country,
                City = x.City,
                State = x.State,
                AccountId = x.AccountId,
                TenantId = x.TenantId,
                Zip = x.Zip,
                Role = x.Role,
                SubRole = x.SubRole
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (contactDetailsDto == null)
        {
            return new NotFound();
        }

        return contactDetailsDto;
    }

    public async Task<OneOf<ContactDetailsDto, NotFound>> UpdateContactStatusAsync(Guid contactId, Guid accountId,
        Status status,
        CancellationToken cancellationToken = default)
    {
        var currentTenantId = _currentUserService.TenantId;

        var contact = await _context
            .Contacts
            .Where(x => x.AccountId == accountId && x.Id == contactId && x.TenantId == currentTenantId)
            .SingleOrDefaultAsync(cancellationToken);

        if (contact == null)
        {
            return new NotFound();
        }

        switch (status)
        {
            case Status.Deactivated:
                contact.DeactivationDateTimeUtc = _dateTimeService.UtcNow;
                contact.ReactivationDateTimeUtc = null;
                break;
            case Status.Active:
                contact.ReactivationDateTimeUtc = _dateTimeService.UtcNow;
                contact.DeactivationDateTimeUtc = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        contact.Status = status;

        var now = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        var activityLog = status switch
        {
            Status.Active => new ContactActivityLog
            {
                TenantId = _currentUserService.TenantId,
                ContactId = contact.Id,
                Date = now,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ContactReactivatedEvent(_currentUserService.UserId,
                        now,
                        currentUserInfo.FullName,
                        currentUserInfo.Roles)),
                EventType = ContactEventType.ContactReactivated
            },
            Status.Deactivated => new ContactActivityLog
            {
                TenantId = _currentUserService.TenantId,
                ContactId = contact.Id,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ContactDeactivatedEvent(_currentUserService.UserId,
                        now,
                        currentUserInfo.FullName,
                        currentUserInfo.Roles)),
                EventType = ContactEventType.ContactDeactivated
            },
            _ => throw new InvalidOperationException()
        };

        await _context.ContactActivityLogs.AddAsync(activityLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Contact ({contactId}) status was changed to {newStatus} by {@userId}",
            _dateTimeService.UtcNow,
            contact.Id,
            status,
            _currentUserService.UserId);

        return contact.Adapt<ContactDetailsDto>();
    }
}

using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Extensions;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Common;

namespace TaxBeacon.Accounts.Contacts;

public class ContactService: IContactService
{
    private readonly ILogger<ContactService> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly IImmutableDictionary<(ContactEventType, uint), IActivityFactory<ContactEventType>>
        _contactActivityFactories;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAccountDbContext _context;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;

    public ContactService(ILogger<ContactService> logger,
        ICurrentUserService currentUserService,
        IAccountDbContext context,
        IDateTimeService dateTimeService,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IEnumerable<IActivityFactory<ContactEventType>> contactActivityFactories)
    {
        _logger = logger;
        _dateTimeService = dateTimeService;
        _contactActivityFactories = contactActivityFactories?
                                        .ToImmutableDictionary(x => (x.EventType, x.Revision))
                                    ?? ImmutableDictionary<(ContactEventType, uint), IActivityFactory<ContactEventType>>
                                        .Empty;
        _currentUserService = currentUserService;
        _context = context;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
    }

    public IQueryable<ContactDto> QueryContacts() =>
        _context.Contacts
            .Where(x => x.TenantId == _currentUserService.TenantId)
            .ProjectToType<ContactDto>();

    public IQueryable<AccountContactDto> QueryAccountContacts(Guid accountId) =>
        _context.AccountContacts
            .Where(ac => ac.AccountId == accountId && ac.TenantId == _currentUserService.TenantId)
            .ProjectToType<AccountContactDto>();

    public async Task<OneOf<ContactDto, NotFound>> GetContactDetailsAsync(Guid contactId,
        CancellationToken cancellationToken = default)
    {
        var contactDetailsDto = await _context
            .Contacts
            .Where(c => c.Id == contactId && c.TenantId == _currentUserService.TenantId)
            .ProjectToType<ContactDto>()
            .SingleOrDefaultAsync(cancellationToken);

        return contactDetailsDto is null ? new NotFound() : contactDetailsDto;
    }

    public async Task<OneOf<AccountContactDto, NotFound>> GetAccountContactDetailsAsync(Guid accountId,
        Guid contactId,
        CancellationToken cancellationToken = default)
    {
        var contactDetailsDto = await _context
            .AccountContacts
            .Where(ac =>
                ac.AccountId == accountId
                && ac.ContactId == contactId
                && ac.TenantId == _currentUserService.TenantId)
            .ProjectToType<AccountContactDto>()
            .SingleOrDefaultAsync(cancellationToken);

        return contactDetailsDto is null ? new NotFound() : contactDetailsDto;
    }

    public async Task<OneOf<AccountContactDto, NotFound>> UpdateAccountContactStatusAsync(Guid accountId,
        Guid contactId,
        Status status,
        CancellationToken cancellationToken = default)
    {
        var accountContact = await _context
            .AccountContacts
            .Where(ac =>
                ac.AccountId == accountId
                && ac.ContactId == contactId
                && ac.TenantId == _currentUserService.TenantId)
            .SingleOrDefaultAsync(cancellationToken);

        if (accountContact is null)
        {
            return new NotFound();
        }

        switch (status)
        {
            case Status.Deactivated:
                accountContact.DeactivationDateTimeUtc = _dateTimeService.UtcNow;
                accountContact.ReactivationDateTimeUtc = null;
                break;
            case Status.Active:
                accountContact.ReactivationDateTimeUtc = _dateTimeService.UtcNow;
                accountContact.DeactivationDateTimeUtc = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        accountContact.Status = status;
        var now = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        var activityLog = status switch
        {
            Status.Active => new AccountContactActivityLog
            {
                TenantId = _currentUserService.TenantId,
                ContactId = accountContact.ContactId,
                AccountId = accountContact.AccountId,
                Date = now,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ContactReactivatedEvent(_currentUserService.UserId,
                        currentUserInfo.Roles,
                        currentUserInfo.FullName,
                        now
                    )),
                EventType = ContactEventType.ContactReactivated
            },
            Status.Deactivated => new AccountContactActivityLog
            {
                TenantId = _currentUserService.TenantId,
                ContactId = accountContact.ContactId,
                AccountId = accountContact.AccountId,
                Date = now,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ContactDeactivatedEvent(_currentUserService.UserId,
                        currentUserInfo.Roles,
                        currentUserInfo.FullName,
                        now)),
                EventType = ContactEventType.ContactDeactivated
            },
            _ => throw new InvalidOperationException()
        };

        await _context.AccountContactActivityLogs.AddAsync(activityLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "{dateTime} - Contact ({contactId}) status in account ({accountId}) was changed to {newStatus} by {userId}",
            _dateTimeService.UtcNow,
            accountContact.ContactId,
            accountContact.AccountId,
            status,
            _currentUserService.UserId);

        return accountContact.Adapt<AccountContactDto>();
    }

    public async Task<OneOf<AccountContactDto, NotFound>> CreateNewContactAsync(Guid accountId,
        CreateContactDto createContactDto,
        CancellationToken cancellationToken = default)
    {
        var accountName = await _context.Accounts
            .Where(a => a.Id == accountId && a.TenantId == _currentUserService.TenantId)
            .Select(a => a.Name)
            .SingleOrDefaultAsync(cancellationToken);

        if (string.IsNullOrEmpty(accountName))
        {
            return new NotFound();
        }

        var newContact = createContactDto.Adapt<Contact>();
        newContact.Id = Guid.NewGuid();
        newContact.TenantId = _currentUserService.TenantId;
        var newAccountContact = new AccountContact
        {
            AccountId = accountId,
            Contact = newContact,
            Status = Status.Active,
            Type = createContactDto.Type.Name
        };

        var currentUserInfo = _currentUserService.UserInfo;
        await _context.Contacts.AddAsync(newContact, cancellationToken);
        await _context.AccountContacts.AddAsync(newAccountContact, cancellationToken);
        await _context.ContactActivityLogs.AddRangeAsync(new ContactActivityLog[]
        {
            new()
            {
                TenantId = _currentUserService.TenantId,
                ContactId = newContact.Id,
                Date = _dateTimeService.UtcNow,
                EventType = ContactEventType.ContactCreated,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ContactCreatedEvent(_currentUserService.UserId,
                        currentUserInfo.Roles,
                        currentUserInfo.FullName,
                        _dateTimeService.UtcNow
                    )),
            },
            new()
            {
                TenantId = _currentUserService.TenantId,
                ContactId = newContact.Id,
                Date = _dateTimeService.UtcNow,
                EventType = ContactEventType.ContactAssignedToAccount,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ContactAssignedToAccountEvent(_currentUserService.UserId,
                        currentUserInfo.Roles,
                        currentUserInfo.FullName,
                        _dateTimeService.UtcNow,
                        accountId,
                        accountName
                    )),
            }
        }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Contact ({contactId}) was created by {userId}",
            _dateTimeService.UtcNow,
            newContact.Id,
            _currentUserService.UserId);

        return newAccountContact.Adapt<AccountContactDto>();
    }

    public async Task<OneOf<Success, NotFound, Conflict>> AssignContactToAccountAsync(Guid accountId,
        Guid contactId,
        ContactType contactType,
        CancellationToken cancellationToken = default)
    {
        if (await _context.AccountContacts.AnyAsync(ac => ac.AccountId == accountId && ac.ContactId == contactId,
                cancellationToken))
        {
            return new Conflict();
        }

        var accountName = await _context.Accounts
            .Where(a => a.Id == accountId && a.TenantId == _currentUserService.TenantId)
            .Select(a => a.Name)
            .SingleOrDefaultAsync(cancellationToken);

        var contactExists = await _context.Contacts
            .AnyAsync(c => c.Id == contactId && c.TenantId == _currentUserService.TenantId,
                cancellationToken);

        if (string.IsNullOrEmpty(accountName) || !contactExists)
        {
            return new NotFound();
        }

        var currentUserInfo = _currentUserService.UserInfo;
        _context.AccountContacts
            .Add(
                new()
                {
                    TenantId = _currentUserService.TenantId,
                    ContactId = contactId,
                    AccountId = accountId,
                    Type = contactType.Name,
                    Status = Status.Active
                });
        _context.ContactActivityLogs.Add(new()
        {
            TenantId = _currentUserService.TenantId,
            ContactId = contactId,
            Date = _dateTimeService.UtcNow,
            EventType = ContactEventType.ContactAssignedToAccount,
            Revision = 1,
            Event = JsonSerializer.Serialize(
                new ContactAssignedToAccountEvent(_currentUserService.UserId,
                    currentUserInfo.Roles,
                    currentUserInfo.FullName,
                    _dateTimeService.UtcNow,
                    accountId,
                    accountName
                )),
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "{dateTime} - Contact ({contactId}) was assigned to account ({accountId}) by user {userId}",
            _dateTimeService.UtcNow,
            contactId,
            accountId,
            _currentUserService.UserId);

        return new Success();
    }

    public async Task<OneOf<Success, NotFound, InvalidOperation>> UnassociateContactWithAccount(Guid accountId,
        Guid contactId, CancellationToken cancellation)
    {
        var currentUserInfo = _currentUserService.UserInfo;
        var tenantId = _currentUserService.TenantId;
        var links = await _context.AccountContacts.Where(x => x.ContactId == contactId && x.TenantId == tenantId)
            .Select(x => new { Link = x, AccountName = x.Account.Name })
            .ToListAsync(cancellation);

        var accountContactLink = links?.FirstOrDefault(x => x.Link.AccountId == accountId);
        if (links == null || accountContactLink == null)
        {
            return new NotFound();
        }

        if (links.Count == 1)
            return new InvalidOperation("Contact should be always linked to at least one account.");

        _context.AccountContacts.Remove(accountContactLink.Link);
        _context.ContactActivityLogs.Add(new()
        {
            TenantId = _currentUserService.TenantId,
            ContactId = contactId,
            Date = _dateTimeService.UtcNow,
            EventType = ContactEventType.ContactUnassociatedWithAccount,
            Revision = 1,
            Event = JsonSerializer.Serialize(
                new ContactUnassociatedWithAccountEvent(_currentUserService.UserId,
                    currentUserInfo.Roles,
                    currentUserInfo.FullName,
                    _dateTimeService.UtcNow,
                    accountId,
                    accountContactLink.AccountName
                )),
        });

        await _context.SaveChangesAsync(cancellation);

        return new Success();
    }

    public async Task<byte[]> ExportContactsAsync(FileType fileType,
        CancellationToken cancellationToken = default)
    {
        var list = await _context
            .Contacts
            .Where(a => a.TenantId == _currentUserService.TenantId)
            .OrderBy(a => a.LastName)
            .ThenBy(a => a.FirstName)
            .ProjectToType<ContactExportModel>()
            .ToListAsync(cancellationToken);

        list.ForEach(e =>
        {
            e.AccountsView = string.Join(", ", e.AccountsNames);
            e.PhonesView = string.Join(Environment.NewLine, e.Phones.Select(p =>
            {
                var number = p.Number.ApplyPhoneMask();
                return string.IsNullOrEmpty(p.Extension)
                    ? $"{p.Type}: {number}"
                    : $"{p.Type}: {number}, {p.Extension}";
            }));
            e.LinkedContactsView = string.Join(", ", e.LinkedContacts.Select(l => $"{l?.FullName} ({l?.Email})"));
        });

        _logger.LogInformation("{dateTime} - Contacts export was executed by {userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(list);
    }

    public async Task<OneOf<byte[], NotFound>> ExportAccountContactsAsync(Guid accountId,
        FileType fileType, CancellationToken cancellationToken = default)
    {
        var isAccountExists = await _context.Accounts
            .AnyAsync(a => a.Id == accountId && a.TenantId == _currentUserService.TenantId,
                cancellationToken);

        if (!isAccountExists)
        {
            return new NotFound();
        }

        var list = await _context
            .AccountContacts
            .OrderBy(ac => ac.Contact.LastName)
            .ThenBy(ac => ac.Contact.FirstName)
            .Where(ac => ac.TenantId == _currentUserService.TenantId && ac.AccountId == accountId)
            .ProjectToType<AccountContactExportModel>()
            .ToListAsync(cancellationToken);

        list.ForEach(e =>
        {
            e.AccountsView = string.Join(", ", e.AccountsNames);
            e.PhonesView = string.Join(Environment.NewLine, e.Phones.Select(p =>
            {
                var number = p.Number.ApplyPhoneMask();
                return string.IsNullOrEmpty(p.Extension)
                    ? $"{p.Type}: {number}"
                    : $"{p.Type}: {number}, {p.Extension}";
            }));
            e.LinkedContactsView = string.Join(", ", e.LinkedContacts.Select(l => $"{l?.FullName} ({l?.Email})"));
        });

        _logger.LogInformation("{dateTime} - Account contacts export was executed by {userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(list);
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetContactActivitiesAsync(Guid contactId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        page = page == 0 ? 1 : page;
        pageSize = pageSize == 0 ? 10 : pageSize;

        if (!await _context.Contacts
                .AnyAsync(c => c.Id == contactId
                               && c.TenantId == _currentUserService.TenantId,
                    cancellationToken))
        {
            return new NotFound();
        }

        var contactActivityLogs = _context.ContactActivityLogs
            .Where(ca => ca.ContactId == contactId && ca.TenantId == _currentUserService.TenantId);

        var count = await contactActivityLogs.CountAsync(cancellationToken);

        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await contactActivityLogs
            .OrderByDescending(x => x.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ActivityDto(pageCount,
            activities.Select(x => _contactActivityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetAccountContactActivitiesAsync(Guid accountId,
        Guid contactId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        page = page == 0 ? 1 : page;
        pageSize = pageSize == 0 ? 10 : pageSize;

        var accountContactExists = await _context.AccountContacts
            .AnyAsync(ac => ac.TenantId == _currentUserService.TenantId
                            && ac.AccountId == accountId
                            && ac.ContactId == contactId,
                cancellationToken);

        if (!accountContactExists)
        {
            return new NotFound();
        }

        var contactActivityLogs = _context.ContactActivityLogs
            .Where(ca => ca.ContactId == contactId && ca.TenantId == _currentUserService.TenantId)
            .ProjectToType<BaseActivityLog<ContactEventType>>();

        var accountContactActivityLogs = _context.AccountContactActivityLogs
            .Where(aca => aca.TenantId == _currentUserService.TenantId
                          && aca.AccountId == accountId
                          && aca.ContactId == contactId)
            .ProjectToType<BaseActivityLog<ContactEventType>>();

        var activityLogs = contactActivityLogs.Concat(accountContactActivityLogs);

        var count = await activityLogs.CountAsync(cancellationToken);

        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await activityLogs
            .OrderByDescending(x => x.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ActivityDto(pageCount,
            activities.Select(x => _contactActivityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }

    public async Task<OneOf<IQueryable<LinkedContactDetailsDto>, NotFound>> QueryLinkedContacts(Guid contactId,
        CancellationToken cancellationToken)
    {
        var contactExists = await _context.Contacts
            .AnyAsync(c => c.Id == contactId
                           && c.TenantId == _currentUserService.TenantId,
                cancellationToken);

        if (!contactExists)
        {
            return new NotFound();
        }

        var linkedContacts = _context.LinkedContacts
            .Where(lc => lc.SourceContactId == contactId && lc.TenantId == _currentUserService.TenantId)
            .Select(lc => new LinkedContactDetailsDto
            {
                SourceContactId = lc.SourceContactId,
                Id = lc.RelatedContactId,
                Name = lc.RelatedContact.FullName,
                FirstName = lc.RelatedContact.FirstName,
                LastName = lc.RelatedContact.LastName,
                Comment = lc.Comment,
                Email = lc.RelatedContact.Email,
                AccountNames = lc.RelatedContact.Accounts.Select(ac => ac.Account.Name),
            });

        return OneOf<IQueryable<LinkedContactDetailsDto>, NotFound>.FromT0(linkedContacts);
    }

    public async Task<OneOf<Success, NotFound>> UnlinkContactFromContactAsync(Guid sourceContactId,
        Guid relatedContactId,
        CancellationToken cancellationToken)
    {
        var currentUserInfo = _currentUserService.UserInfo;
        var tenantId = _currentUserService.TenantId;
        var currentDateTime = _dateTimeService.UtcNow;

        var linkedContacts = await _context.LinkedContacts
            .Where(lc => (lc.SourceContactId == sourceContactId && lc.RelatedContactId == relatedContactId
                          || lc.SourceContactId == relatedContactId && lc.RelatedContactId == sourceContactId)
                         && lc.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        if (!linkedContacts.Any())
            return new NotFound();

        var sourceContactName = await _context.Contacts
            .Where(c => c.Id == sourceContactId && c.TenantId == tenantId)
            .Select(sc => sc.FullName)
            .FirstAsync(cancellationToken);

        var relatedContactName = await _context.Contacts
            .Where(c => c.Id == relatedContactId && c.TenantId == tenantId)
            .Select(sc => sc.FullName)
            .FirstAsync(cancellationToken);

        _context.LinkedContacts.RemoveRange(linkedContacts);

        _context.ContactActivityLogs.AddRange(
            new()
            {
                TenantId = tenantId,
                ContactId = sourceContactId,
                Date = _dateTimeService.UtcNow,
                EventType = ContactEventType.ContactUnlinkedFromContact,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ContactUnlinkedFromContactEvent(_currentUserService.UserId,
                        currentUserInfo.Roles,
                        currentUserInfo.FullName,
                        currentDateTime,
                        relatedContactId,
                        relatedContactName
                    )),
            },
            new()
            {
                TenantId = tenantId,
                ContactId = relatedContactId,
                Date = _dateTimeService.UtcNow,
                EventType = ContactEventType.ContactUnlinkedFromContact,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ContactUnlinkedFromContactEvent(_currentUserService.UserId,
                        currentUserInfo.Roles,
                        currentUserInfo.FullName,
                        currentDateTime,
                        sourceContactId,
                        sourceContactName
                    )),
            });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "{dateTime} - Contact ({sourceContactId}) was unlinked from contact ({relatedContactId}) by user {userId}",
            _dateTimeService.UtcNow,
            sourceContactId,
            relatedContactId,
            _currentUserService.UserId);

        return new Success();
    }

    public async Task<OneOf<AccountContactDto, NotFound>> UpdateAccountContactAsync(Guid contactId,
        Guid accountId,
        UpdateAccountContactDto updateContact,
        CancellationToken cancellationToken = default)
    {
        var accountContact = await _context
            .AccountContacts
            .Include(x => x.Contact)
            .ThenInclude(x => x.Phones)
            .SingleOrDefaultAsync(ac => ac.ContactId == contactId
                                        && ac.AccountId == accountId
                                        && ac.TenantId == _currentUserService.TenantId,
                cancellationToken);

        if (accountContact is null)
        {
            return new NotFound();
        }

        var previousValues = JsonSerializer.Serialize(accountContact.Adapt<UpdateAccountContactDto>());
        var userInfo = _currentUserService.UserInfo;

        var contactUpdatedEventTime = _dateTimeService.UtcNow;

        _context.ContactActivityLogs.Add(new ContactActivityLog
        {
            ContactId = contactId,
            TenantId = _currentUserService.TenantId,
            Date = contactUpdatedEventTime,
            Revision = 1,
            EventType = ContactEventType.ContactUpdated,
            Event = JsonSerializer.Serialize(new ContactUpdatedEvent(
                _currentUserService.UserId,
                userInfo.Roles ?? string.Empty,
                userInfo.FullName,
                contactUpdatedEventTime,
                previousValues,
                JsonSerializer.Serialize(updateContact)))
        });

        if (accountContact.Type != updateContact.Type.Name)
        {
            var contactTypeUpdateEventTime = _dateTimeService.UtcNow;
            _context.AccountContactActivityLogs.Add(new AccountContactActivityLog()
            {
                ContactId = contactId,
                TenantId = _currentUserService.TenantId,
                AccountId = accountId,
                Date = contactTypeUpdateEventTime,
                Revision = 1,
                EventType = ContactEventType.ContactTypeUpdated,
                Event = JsonSerializer.Serialize(new ContactTypeUpdatedEvent(
                    _currentUserService.UserId,
                    userInfo.Roles ?? string.Empty,
                    userInfo.FullName,
                    contactTypeUpdateEventTime,
                    accountContact.Type,
                    updateContact.Type.Name))
            });
        }

        updateContact.Adapt(accountContact);

        var contact = accountContact.Contact;
        var currentPhoneIds = contact.Phones.Select(p => p.Id).ToArray();
        var newPhoneIds = updateContact.Phones.Select(p => p.Id);
        var phonesToRemove = contact.Phones
            .Where(p =>
                currentPhoneIds.Except(newPhoneIds).Contains(p.Id))
            .ToList();
        var phonesToAdd = updateContact.Phones
            .Where(p =>
                newPhoneIds.Except(currentPhoneIds).Contains(p.Id))
            .Select(p =>
            {
                var phone = p.Adapt<ContactPhone>();
                phone.ContactId = contact.Id;
                phone.TenantId = _currentUserService.TenantId;
                return phone;
            })
            .ToList();
        var phoneDtosToUpdate = updateContact.Phones
            .Where(p => newPhoneIds.Intersect(currentPhoneIds).Contains(p.Id))
            .ToDictionary(p => p.Id);

        foreach (var phone in contact.Phones.Where(p => phoneDtosToUpdate.ContainsKey(p.Id)))
        {
            _context.ContactPhones.Update(phoneDtosToUpdate[phone.Id].Adapt(phone));
        }

        _context.ContactPhones.RemoveRange(phonesToRemove);
        await _context.ContactPhones.AddRangeAsync(phonesToAdd, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Account contact ({contactId}) was updated by {userId}",
            contactUpdatedEventTime,
            contactId,
            _currentUserService.UserId);
        return accountContact.Adapt<AccountContactDto>();
    }

    public async Task<OneOf<ContactDto, NotFound>> UpdateContactAsync(Guid contactId,
        UpdateContactDto updateContact,
        CancellationToken cancellationToken = default)
    {
        var contact = await _context
            .Contacts
            .Include(x => x.Phones)
            .SingleOrDefaultAsync(ac => ac.Id == contactId
                                        && ac.TenantId == _currentUserService.TenantId, cancellationToken);

        if (contact is null)
        {
            return new NotFound();
        }

        var previousValues = JsonSerializer.Serialize(contact.Adapt<UpdateContactDto>());
        var userInfo = _currentUserService.UserInfo;
        var eventDateTime = _dateTimeService.UtcNow;

        _context.ContactActivityLogs.Add(new ContactActivityLog
        {
            ContactId = contactId,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = ContactEventType.ContactUpdated,
            Event = JsonSerializer.Serialize(new ContactUpdatedEvent(
                _currentUserService.UserId,
                userInfo.Roles ?? string.Empty,
                userInfo.FullName,
                eventDateTime,
                previousValues,
                JsonSerializer.Serialize(updateContact)))
        });

        updateContact.Adapt(contact);

        var currentPhoneIds = contact.Phones.Select(p => p.Id).ToArray();
        var newPhoneIds = updateContact.Phones.Select(p => p.Id);
        var phonesToRemove = contact.Phones
            .Where(p =>
                currentPhoneIds.Except(newPhoneIds).Contains(p.Id))
            .ToList();
        var phonesToAdd = updateContact.Phones
            .Where(p =>
                newPhoneIds.Except(currentPhoneIds).Contains(p.Id))
            .Select(p =>
            {
                var phone = p.Adapt<ContactPhone>();
                phone.ContactId = contact.Id;
                phone.TenantId = _currentUserService.TenantId;
                return phone;
            })
            .ToList();
        var phoneDtosToUpdate = updateContact.Phones
            .Where(p => newPhoneIds.Intersect(currentPhoneIds).Contains(p.Id))
            .ToDictionary(p => p.Id);

        foreach (var phone in contact.Phones.Where(p => phoneDtosToUpdate.ContainsKey(p.Id)))
        {
            _context.ContactPhones.Update(phoneDtosToUpdate[phone.Id].Adapt(phone));
        }

        _context.ContactPhones.RemoveRange(phonesToRemove);
        await _context.ContactPhones.AddRangeAsync(phonesToAdd, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Contact ({contactId}) was updated by {userId}",
            eventDateTime,
            contactId,
            _currentUserService.UserId);

        return contact.Adapt<ContactDto>();
    }

    public async Task<OneOf<Success, NotFound, Conflict>> LinkContactToContactAsync(Guid sourceContactId,
        Guid relatedContactId,
        string comment,
        CancellationToken cancellationToken = default)
    {
        var contactsNames = await _context.Contacts
            .Where(c => (c.Id == sourceContactId || c.Id == relatedContactId)
                        && c.TenantId == _currentUserService.TenantId)
            .Select(c => new { c.Id, c.FullName })
            .ToListAsync(cancellationToken);

        if (contactsNames.Count != 2)
        {
            return new NotFound();
        }

        var isContactsLinked = await _context.LinkedContacts
            .AnyAsync(c => (
                               c.SourceContactId == sourceContactId && c.RelatedContactId == relatedContactId
                               || c.SourceContactId == relatedContactId && c.RelatedContactId == sourceContactId)
                           && c.TenantId == _currentUserService.TenantId,
                cancellationToken);

        if (isContactsLinked)
        {
            return new Conflict();
        }

        _context.LinkedContacts.AddRange(
            new()
            {
                SourceContactId = sourceContactId,
                RelatedContactId = relatedContactId,
                TenantId = _currentUserService.TenantId,
                Comment = comment
            },
            new()
            {
                SourceContactId = relatedContactId,
                RelatedContactId = sourceContactId,
                TenantId = _currentUserService.TenantId,
                Comment = comment
            });

        var userInfo = _currentUserService.UserInfo;

        _context.ContactActivityLogs.AddRange(
            new()
            {
                ContactId = sourceContactId,
                TenantId = _currentUserService.TenantId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                EventType = ContactEventType.ContactLinkedToContact,
                Event = JsonSerializer.Serialize(new ContactLinkedToContactEvent(
                    _currentUserService.UserId,
                    userInfo.Roles,
                    userInfo.FullName,
                    _dateTimeService.UtcNow,
                    relatedContactId,
                    contactsNames.Single(c => c.Id == relatedContactId).FullName))
            },
            new()
            {
                ContactId = relatedContactId,
                TenantId = _currentUserService.TenantId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                EventType = ContactEventType.ContactLinkedToContact,
                Event = JsonSerializer.Serialize(new ContactLinkedToContactEvent(
                    _currentUserService.UserId,
                    userInfo.Roles,
                    userInfo.FullName,
                    _dateTimeService.UtcNow,
                    sourceContactId,
                    contactsNames.Single(c => c.Id == sourceContactId).FullName))
            }
        );

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "{dateTime} - Contact ({sourceContactId}) was linked to contact ({relatedContactId}) by {userId}",
            _dateTimeService.UtcNow,
            sourceContactId,
            relatedContactId,
            _currentUserService.UserId);

        return new Success();
    }
}

using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Text.Json;
using TaxBeacon.Accounts.Locations.Activities.Factories;
using TaxBeacon.Accounts.Locations.Activities.Models;
using TaxBeacon.Accounts.Locations.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Extensions;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Accounts;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Locations;

public class LocationService: ILocationService
{
    private readonly ILogger<LocationService> _logger;
    private readonly IAccountDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly IImmutableDictionary<(LocationEventType, uint), ILocationActivityFactory> _locationActivityFactories;
    private readonly ImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    private readonly IDateTimeFormatter _dateTimeFormatter;

    public LocationService(IAccountDbContext context,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<LocationService> logger,
        IEnumerable<ILocationActivityFactory> locationActivityFactories,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IDateTimeFormatter dateTimeFormatter)
    {
        _context = context;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _logger = logger;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _locationActivityFactories = locationActivityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                                     ?? ImmutableDictionary<(LocationEventType, uint), ILocationActivityFactory>.Empty;
        _dateTimeFormatter = dateTimeFormatter;
    }

    public OneOf<IQueryable<LocationDto>, NotFound> QueryLocations(Guid accountId)
    {
        var tenantId = _currentUserService.TenantId;

        var accountExists = _context.Accounts.Any(acc => acc.Id == accountId && acc.TenantId == tenantId);

        if (!accountExists)
        {
            return new NotFound();
        }

        var locations = _context.Locations
            .Where(l => l.AccountId == accountId && l.TenantId == _currentUserService.TenantId)
            .ProjectToType<LocationDto>();

        return OneOf<IQueryable<LocationDto>, NotFound>.FromT0(locations);
    }

    public async Task<OneOf<LocationDetailsDto, NotFound>> GetLocationDetailsAsync(Guid accountId, Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var location = await _context
            .Locations
            .Include(e => e.NaicsCode)
            .Include(l => l.Phones)
            .Where(l => l.TenantId == _currentUserService.TenantId && l.Id == locationId && l.AccountId == accountId)
            .ProjectToType<LocationDetailsDto>()
            .SingleOrDefaultAsync(cancellationToken);

        return location is null ? new NotFound() : location;
    }

    public async Task<OneOf<LocationDetailsDto, NotFound>> CreateNewLocationAsync(Guid accountId,
        CreateLocationDto createLocationDto,
        CancellationToken cancellationToken = default)
    {
        if (!await _context.Accounts.AnyAsync(a => a.TenantId == _currentUserService.TenantId && a.Id == accountId,
                cancellationToken))
        {
            return new NotFound();
        }

        var location = createLocationDto.Adapt<Location>();
        location.Id = Guid.NewGuid();
        location.AccountId = accountId;
        location.TenantId = _currentUserService.TenantId;
        location.Status = Status.Active;

        var eventDateTime = _dateTimeService.UtcNow;
        var currentUserInfo = _currentUserService.UserInfo;

        await _context.Locations.AddAsync(location, cancellationToken);
        await _context.LocationPhones.AddRangeAsync(
            createLocationDto.Phones.Select(p => new LocationPhone
            {
                Id = Guid.NewGuid(),
                Type = p.Type,
                Number = p.Number,
                Extension = p.Extension,
                LocationId = location.Id,
                TenantId = _currentUserService.TenantId
            }), cancellationToken);
        await _context.EntityLocations.AddRangeAsync(
            createLocationDto.EntitiesIds.Select(id => new EntityLocation
            {
                LocationId = location.Id,
                EntityId = id,
                TenantId = _currentUserService.TenantId
            }),
            cancellationToken);
        await _context.LocationActivityLogs.AddAsync(new LocationActivityLog
        {
            LocationId = location.Id,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = LocationEventType.LocationCreated,
            Event = JsonSerializer.Serialize(new LocationCreatedEvent(
                _currentUserService.UserId,
                currentUserInfo.Roles,
                currentUserInfo.FullName,
                eventDateTime))
        }, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Location ({createdLocationId}) was created by {userId}",
            eventDateTime,
            location.Id,
            _currentUserService.UserId);

        return location.Adapt<LocationDetailsDto>();
    }

    public async Task<OneOf<LocationDetailsDto, NotFound>> UpdateLocationStatusAsync(Guid accountId,
        Guid locationId,
        Status status,
        CancellationToken cancellationToken = default)
    {
        var currentTenantId = _currentUserService.TenantId;
        var location = await _context
            .Locations
            .Where(l => l.Id == locationId && l.TenantId == currentTenantId && l.AccountId == accountId)
            .SingleOrDefaultAsync(cancellationToken);

        if (location == null)
        {
            return new NotFound();
        }

        var eventDateTime = _dateTimeService.UtcNow;
        switch (status)
        {
            case Status.Deactivated:
                location.DeactivationDateTimeUtc = eventDateTime;
                location.ReactivationDateTimeUtc = null;
                break;
            case Status.Active:
                location.ReactivationDateTimeUtc = eventDateTime;
                location.DeactivationDateTimeUtc = null;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }

        location.Status = status;
        var currentUserInfo = _currentUserService.UserInfo;

        var activityLog = status switch
        {
            Status.Active => new LocationActivityLog
            {
                TenantId = _currentUserService.TenantId,
                Date = eventDateTime,
                Revision = 1,
                LocationId = locationId,
                Event = JsonSerializer.Serialize(
                    new LocationReactivatedEvent(_currentUserService.UserId,
                        eventDateTime,
                        currentUserInfo.FullName,
                        currentUserInfo.Roles)),
                EventType = LocationEventType.LocationReactivated,
            },
            Status.Deactivated => new LocationActivityLog
            {
                TenantId = _currentUserService.TenantId,
                Date = eventDateTime,
                Revision = 1,
                LocationId = locationId,
                Event = JsonSerializer.Serialize(
                    new LocationDeactivatedEvent(_currentUserService.UserId,
                        eventDateTime,
                        currentUserInfo.FullName,
                        currentUserInfo.Roles)),
                EventType = LocationEventType.LocationDeactivated,
            },
            _ => throw new InvalidOperationException()
        };

        await _context.LocationActivityLogs.AddAsync(activityLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{dateTime} - Location ({locationId}) status was changed to {newStatus} by {userId}",
            eventDateTime,
            locationId,
            status,
            _currentUserService.UserId);

        return await GetLocationDetailsAsync(accountId, locationId, cancellationToken);
    }

    public async Task<OneOf<LocationDetailsDto, NotFound>> UpdateLocationAsync(Guid accountId,
        Guid locationId,
        UpdateLocationDto updateLocation,
        CancellationToken cancellationToken = default)
    {
        var location = await _context
                    .Locations
                    .Include(l => l.Phones)
                    .SingleOrDefaultAsync(l => l.Id == locationId
                                               && l.TenantId == _currentUserService.TenantId
                                               && l.AccountId == accountId, cancellationToken);

        if (location is null)
        {
            return new NotFound();
        }

        updateLocation.Adapt(location);

        var currentPhoneIds = location.Phones.Select(p => p.Id).ToArray();
        var newPhoneIds = updateLocation.Phones.Select(p => p.Id);
        var phonesToRemove = location.Phones
            .Where(p =>
                currentPhoneIds.Except(newPhoneIds).Contains(p.Id))
            .ToList();
        var phonesToAdd = updateLocation.Phones
            .Where(p =>
                newPhoneIds.Except(currentPhoneIds).Contains(p.Id))
            .Select(p =>
            {
                var phone = p.Adapt<LocationPhone>();
                phone.LocationId = location.Id;
                phone.TenantId = _currentUserService.TenantId;
                return phone;
            })
            .ToList();

        var phoneDtosToUpdate = updateLocation.Phones
            .Where(p => newPhoneIds.Intersect(currentPhoneIds).Contains(p.Id))
            .ToDictionary(p => p.Id);

        foreach (var phone in location.Phones.Where(p => phoneDtosToUpdate.ContainsKey(p.Id)))
        {
            _context.LocationPhones.Update(phoneDtosToUpdate[phone.Id].Adapt(phone));
        }

        _context.LocationPhones.RemoveRange(phonesToRemove);
        await _context.LocationPhones.AddRangeAsync(phonesToAdd, cancellationToken);

        var previousValues = JsonSerializer.Serialize(location.Adapt<UpdateLocationDto>());
        var userInfo = _currentUserService.UserInfo;
        var eventDateTime = _dateTimeService.UtcNow;
        _context.LocationActivityLogs.Add(new LocationActivityLog
        {
            LocationId = locationId,
            TenantId = _currentUserService.TenantId,
            Date = eventDateTime,
            Revision = 1,
            EventType = LocationEventType.LocationUpdated,
            Event = JsonSerializer.Serialize(new LocationUpdatedEvent(
                _currentUserService.UserId,
                userInfo.Roles ?? string.Empty,
                userInfo.FullName,
                eventDateTime,
                previousValues,
                JsonSerializer.Serialize(updateLocation)))
        });

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Location ({locationId}) was updated by {userId}",
            eventDateTime,
            locationId,
            _currentUserService.UserId);

        return location.Adapt<LocationDetailsDto>();
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid accountId, Guid locationId, int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        page = page == 0 ? 1 : page;
        pageSize = pageSize == 0 ? 10 : pageSize;

        var locationExists = await _context.Locations
            .AnyAsync(l => l.Id == locationId
                           && l.TenantId == _currentUserService.TenantId && l.AccountId == accountId,
                cancellationToken);

        if (!locationExists)
        {
            return new NotFound();
        }

        var locationActivityLogs = _context.LocationActivityLogs
            .Where(l => l.LocationId == locationId && l.TenantId == _currentUserService.TenantId);

        var count = await locationActivityLogs.CountAsync(cancellationToken);

        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await locationActivityLogs
            .OrderByDescending(x => x.Date)
            .Skip((int)((page - 1) * pageSize))
            .Take((int)pageSize)
            .ToListAsync(cancellationToken);

        return new ActivityDto(pageCount,
            activities.Select(x => _locationActivityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }

    public async Task<OneOf<byte[], NotFound>> ExportLocationsAsync(Guid accountId, FileType fileType, CancellationToken cancellationToken = default)
    {
        var isAccountExists = await _context.Accounts
            .AnyAsync(a => a.Id == accountId && a.TenantId == _currentUserService.TenantId,
                cancellationToken);

        if (!isAccountExists)
        {
            return new NotFound();
        }

        var locations = await _context.Locations
            .Where(e => e.TenantId == _currentUserService.TenantId && e.AccountId == accountId)
            .Include(l => l.Phones)
            .Include(l => l.NaicsCode)
            .OrderBy(e => e.Name)
            .ProjectToType<LocationExportModel>()
            .ToListAsync(cancellationToken);

        locations.ForEach(l =>
        {
            l.PhonesView = string.Join("\n", l.Phones.Select(p =>
            {
                var number = l.Country == "United States" ? p.Number.ApplyPhoneMask() : p.Number;
                return string.IsNullOrEmpty(p.Extension)
                    ? $"{p.Type}: {number}"
                    : $"{p.Type}: {number}, {p.Extension}";
            }));
            l.Zip = l.Zip.ApplyZipMask();
            l.EndDateView = _dateTimeFormatter.FormatDate(l.EndDateTimeUtc);
            l.StartDateView = _dateTimeFormatter.FormatDate(l.StartDateTimeUtc);
        });

        _logger.LogInformation("{dateTime} - Locations export was executed by {userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(locations);
    }

    public async Task<OneOf<string, InvalidOperation>> GenerateUniqueLocationIdAsync(CancellationToken cancellationToken)
    {
        var r = new Random();
        var code = "L" + r.Next(1_000_000, 9_999_999).ToString();
        var safeExit = 0;
        var tenantId = _currentUserService.TenantId;
        while (safeExit < 10 && await _context.Locations.AnyAsync(x => x.LocationId == code && x.TenantId == tenantId, cancellationToken))
        {
            code = "L" + r.Next(1_000_000, 9_999_999).ToString();
            safeExit++;
        }
        if (safeExit >= 10)
        {
            _logger.LogError("Failed to generate location Id. Attempts count exceeded the limit.");
            return new InvalidOperation("Failed to generate the code");
        }
        return code;
    }
}

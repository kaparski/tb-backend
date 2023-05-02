using Gridify;
using Mapster;
using Microsoft.Extensions.Logging;
using OneOf.Types;
using OneOf;
using System.Collections.Immutable;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.UserManagement.Services.Activities.Divisions;
using System.Text.Json;
using TaxBeacon.DAL.Entities;
using TaxBeacon.UserManagement.Models.Activities.Tenant;
using TaxBeacon.UserManagement.Models.Activities;

namespace TaxBeacon.UserManagement.Services
{
    public class DivisionsService: IDivisionsService
    {
        private readonly ILogger<DivisionsService> _logger;
        private readonly ITaxBeaconDbContext _context;
        private readonly IDateTimeService _dateTimeService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
        private readonly IDateTimeFormatter _dateTimeFormatter;
        private readonly IImmutableDictionary<(DivisionEventType, uint), IDivisionActivityFactory> _divisionActivityFactories;

        public DivisionsService(ILogger<DivisionsService> logger,
            ITaxBeaconDbContext context,
            IDateTimeService dateTimeService,
            ICurrentUserService currentUserService,
            IEnumerable<IListToFileConverter> listToFileConverters,
            IDateTimeFormatter dateTimeFormatter,
            IEnumerable<IDivisionActivityFactory> divisionActivityFactories)
        {
            _logger = logger;
            _context = context;
            _dateTimeService = dateTimeService;
            _currentUserService = currentUserService;
            _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                    ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
            _dateTimeFormatter = dateTimeFormatter;
            _divisionActivityFactories = divisionActivityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                                         ?? ImmutableDictionary<(DivisionEventType, uint), IDivisionActivityFactory>.Empty;
        }

        public async Task<OneOf<QueryablePaging<DivisionDto>, NotFound>> GetDivisionsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
        {
            var tenantId = _currentUserService.TenantId;
            var divisions = await _context
                .Divisions
                .Where(div => div.TenantId == tenantId)
                .Select(div => new DivisionDto
                {
                    Id = div.Id,
                    Name = div.Name,
                    Description = div.Description,
                    CreatedDateTimeUtc = div.CreatedDateTimeUtc,
                    NumberOfUsers = div.Users.Count(),
                    Departments = string.Join(", ", div.Departments.Select(dep => dep.Name)),
                    Department = div.Departments.Select(dep => dep.Name)
                    .GroupBy(dep => 1)
                    .Select(g => string.Join(string.Empty, g.Select(s => "|" + s + "|")))
                    .FirstOrDefault() ?? string.Empty
                })
                .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

            if (gridifyQuery.Page == 1 || divisions.Query.Any())
            {
                return divisions;
            }

            return new NotFound();
        }

        public async Task<byte[]> ExportDivisionsAsync(FileType fileType,
        CancellationToken cancellationToken)
        {
            var tenantId = _currentUserService.TenantId;
            var exportTenants = await _context
                .Divisions
                .Where(d => d.TenantId == tenantId)
                .Select(div => new DivisionDto
                {
                    Id = div.Id,
                    Name = div.Name,
                    Description = div.Description,
                    CreatedDateTimeUtc = div.CreatedDateTimeUtc,
                    NumberOfUsers = div.Users.Count(),
                    Departments = string.Join(", ", div.Departments.Select(dep => dep.Name)),
                })
                .AsNoTracking()
                .ProjectToType<DivisionExportModel>()
                .ToListAsync(cancellationToken);

            exportTenants.ForEach(t => t.CreatedDateView = _dateTimeFormatter.FormatDate(t.CreatedDateTimeUtc));

            _logger.LogInformation("{dateTime} - Tenants export was executed by {@userId}",
                _dateTimeService.UtcNow,
                _currentUserService.UserId);

            return _listToFileConverters[fileType].Convert(exportTenants);
        }

        public async Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid divisionId, uint page = 1,
            uint pageSize = 10, CancellationToken cancellationToken = default)
        {
            page = page == 0 ? 1 : page;
            pageSize = pageSize == 0 ? 10 : pageSize;

            var division = await _context.Divisions
                .Where(d => d.Id == divisionId && d.TenantId == _currentUserService.TenantId)
                .FirstOrDefaultAsync(cancellationToken);

            if (division is null)
            {
                return new NotFound();
            }

            var divisionActivityLogs = _context.DivisionActivityLogs
                .Where(ua => ua.DivisionId == divisionId && ua.TenantId == _currentUserService.TenantId);

            var count = await divisionActivityLogs.CountAsync(cancellationToken: cancellationToken);

            var pageCount = (uint)Math.Ceiling((double)count / pageSize);

            var activities = await divisionActivityLogs
                .OrderByDescending(x => x.Date)
                .Skip((int)((page - 1) * pageSize))
                .Take((int)pageSize)
                .ToListAsync(cancellationToken);

            return new ActivityDto(pageCount,
                activities.Select(x => _divisionActivityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
        }

        public async Task<OneOf<DivisionDetailsDto, NotFound>> GetDivisionDetailsAsync(Guid divisionId, CancellationToken cancellationToken = default)
        {
            var division = await _context
                .Divisions
                .Include(x => x.Departments)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == _currentUserService.TenantId && x.Id == divisionId, cancellationToken);

            return division is null
                ? new NotFound()
                : division.Adapt<DivisionDetailsDto>();
        }

        public async Task<OneOf<DivisionDto, NotFound>> UpdateDivisionAsync(Guid id, UpdateDivisionDto updateDivisionDto,
    CancellationToken cancellationToken = default)
        {
            var division = await _context.Divisions.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (division is null)
            {
                return new NotFound();
            }

            var previousValues = JsonSerializer.Serialize(division.Adapt<UpdateDivisionDto>());
            var currentUserFullName = (await _context.Users.FindAsync(_currentUserService.UserId, cancellationToken))!.FullName;
            var currentUserRoles = await _context
                .TenantUserRoles
                .Where(x => x.UserId == _currentUserService.UserId && x.TenantId == _currentUserService.TenantId)
                .GroupBy(r => 1, t => t.TenantRole.Role.Name)
                .Select(group => string.Join(", ", group.Select(name => name)))
                .FirstOrDefaultAsync(cancellationToken);
            var eventDateTime = _dateTimeService.UtcNow;

            await _context.DivisionActivityLogs.AddAsync(new DivisionActivityLog
            {
                DivisionId = id,
                TenantId = _currentUserService.TenantId,
                Date = eventDateTime,
                Revision = 1,
                EventType = DivisionEventType.DivisionUpdatedEvent,
                Event = JsonSerializer.Serialize(new DivisionUpdatedEvent(
                    _currentUserService.UserId,
                    currentUserRoles ?? string.Empty,
                    currentUserFullName,
                    eventDateTime,
                    previousValues,
                    JsonSerializer.Serialize(updateDivisionDto)))
            }, cancellationToken);

            updateDivisionDto.Adapt(division);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("{dateTime} - Division ({divisionId}) was updated by {@userId}",
                eventDateTime,
                id,
                _currentUserService.UserId);

            return division.Adapt<DivisionDto>();
        }

        public async Task<OneOf<QueryablePaging<DivisionUserDto>, NotFound>> GetDivisionUsersAsync(Guid divisionId, GridifyQuery gridifyQuery, CancellationToken cancellationToken = default)
        {
            var tenantId = _currentUserService.TenantId;

            var division = await _context.Divisions
                .FirstOrDefaultAsync(d => d.Id == divisionId && d.TenantId == tenantId, cancellationToken);

            if (division is null)
            {
                return new NotFound();
            }

            var users = await _context
                .Users
                .Where(u => u.DivisionId == divisionId && u.TenantUsers.Any(x => x.TenantId == tenantId && x.UserId == u.Id))
                .ProjectToType<DivisionUserDto>()
                .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

            return users;
        }
    }
}

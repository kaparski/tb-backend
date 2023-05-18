using Gridify;
using Gridify.EntityFramework;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Text.Json;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services.Activities;

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
                .OrderBy(d => d.Name)
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
                .Include(x => x.Departments.OrderBy(dep => dep.Name))
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.TenantId == _currentUserService.TenantId && x.Id == divisionId, cancellationToken);

            return division is null
                ? new NotFound()
                : division.Adapt<DivisionDetailsDto>();
        }

        public async Task<OneOf<DivisionDetailsDto, NotFound>> UpdateDivisionAsync(Guid id, UpdateDivisionDto updateDivisionDto,
        CancellationToken cancellationToken = default)
        {
            var division = await _context.Divisions.Include(d => d.Departments).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (division is null)
            {
                return new NotFound();
            }

            var isUpdateBlocked = await _context.Departments
                .Where(d => updateDivisionDto.DepartmentIds.Contains(d.Id)
                    && d.DivisionId != null
                    && d.DivisionId != division.Id)
                .AnyAsync();

            if (isUpdateBlocked)
            {
                return new NotFound();
            }

            var previousValues = JsonSerializer.Serialize(division.Adapt<UpdateDivisionDto>());
            var userInfo = _currentUserService.UserInfo;
            var eventDateTime = _dateTimeService.UtcNow;

            var currentDepsIds = division.Departments.Select(dep => dep.Id).ToList();

            // Removes association with departments
            await _context.Departments
                .Where(dep => currentDepsIds.Except(updateDivisionDto.DepartmentIds).Contains(dep.Id))
                .ForEachAsync(dep => dep.DivisionId = null);

            // Set up association with freshly added departments
            await _context.Departments
                .Where(dep => updateDivisionDto.DepartmentIds.Except(currentDepsIds).Contains(dep.Id))
                .ForEachAsync(dep => dep.DivisionId = id);

            await _context.DivisionActivityLogs.AddAsync(new DivisionActivityLog
            {
                DivisionId = id,
                TenantId = _currentUserService.TenantId,
                Date = eventDateTime,
                Revision = 1,
                EventType = DivisionEventType.DivisionUpdatedEvent,
                Event = JsonSerializer.Serialize(new DivisionUpdatedEvent(
                    _currentUserService.UserId,
                    userInfo.Roles ?? string.Empty,
                    userInfo.FullName,
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

            return await GetDivisionDetailsAsync(id, cancellationToken);
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

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
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;
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

        public IQueryable<DivisionDto> QueryDivisions()
        {
            var items = _context.Divisions.Where(d => d.TenantId == _currentUserService.TenantId);

            var itemDtos = items.GroupJoin(_context.Departments,
                d => d.Id,
                sa => sa.DivisionId,
                (d, departments) => new DivisionDto
                {
                    Id = d.Id,
                    Name = d.Name,
                    Description = d.Description,
                    CreatedDateTimeUtc = d.CreatedDateTimeUtc,
                    NumberOfUsers = d.Users.Count(),
                    DepartmentIds = departments.Select(r => r.Id)
                })
            ;

            return itemDtos;
        }

        public async Task<QueryablePaging<DivisionDto>> GetDivisionsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) => await _context
                .Divisions
                .Where(div => div.TenantId == _currentUserService.TenantId)
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

        public async Task<OneOf<DivisionDetailsDto, NotFound>> GetDivisionDetailsAsync(Guid divisionId,
            CancellationToken cancellationToken = default)
        {
            var divisionDetails = await _context
               .Divisions
               .Include(x => x.Departments.OrderBy(dep => dep.Name))
               .Where(x => x.TenantId == _currentUserService.TenantId && x.Id == divisionId)
               .ProjectToType<DivisionDetailsDto>()
               .AsNoTracking()
               .FirstOrDefaultAsync(cancellationToken);

            return divisionDetails is null ? new NotFound() : divisionDetails;
        }

        public async Task<OneOf<DivisionDetailsDto, NotFound, InvalidOperation>> UpdateDivisionAsync(Guid id,
            UpdateDivisionDto updateDivisionDto, CancellationToken cancellationToken = default)
        {
            var division = await _context.Divisions.Include(d => d.Departments).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (division is null)
            {
                return new NotFound();
            }

            var currentDepsIds = division.Departments.Select(dep => dep.Id).ToList();

            if (updateDivisionDto.DepartmentIds != null)
            {
                var alreadyAssignedDepartments = await _context.Departments
                    .Where(d => updateDivisionDto.DepartmentIds.Contains(d.Id)
                        && d.DivisionId != null
                        && d.DivisionId != division.Id
                        && d.TenantId == _currentUserService.TenantId)
                    .Select(d => d.Name)
                    .ToListAsync(cancellationToken);

                if (alreadyAssignedDepartments.Any())
                {
                    return new InvalidOperation($"Department(s) {string.Join(", ", alreadyAssignedDepartments)} have been assigned to another division");
                }

                // Removes association with departments
                await _context.Departments
                    .Where(dep => currentDepsIds.Except(updateDivisionDto.DepartmentIds).Contains(dep.Id))
                    .ForEachAsync(dep => dep.DivisionId = null, cancellationToken);

                // Set up association with freshly added departments
                await _context.Departments
                    .Where(dep => updateDivisionDto.DepartmentIds.Except(currentDepsIds).Contains(dep.Id))
                    .ForEachAsync(dep => dep.DivisionId = id, cancellationToken);
            }
            else
            {
                // Removes association with departments
                await _context.Departments
                    .Where(dep => dep.DivisionId == id && dep.TenantId == _currentUserService.TenantId)
                    .ForEachAsync(dep => dep.DivisionId = null, cancellationToken);
            }

            var previousValues = JsonSerializer.Serialize(division.Adapt<UpdateDivisionDto>());
            var userInfo = _currentUserService.UserInfo;
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
                    userInfo.Roles ?? string.Empty,
                    userInfo.FullName,
                    eventDateTime,
                    previousValues,
                    JsonSerializer.Serialize(updateDivisionDto)))
            }, cancellationToken);

            division.Name = updateDivisionDto.Name;
            division.Description = updateDivisionDto.Description;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("{dateTime} - Division ({divisionId}) was updated by {@userId}",
                eventDateTime,
                id,
                _currentUserService.UserId);

            return (await GetDivisionDetailsAsync(id, cancellationToken)).AsT0;
        }

        public async Task<OneOf<DivisionDepartmentDto[], NotFound>> GetDivisionDepartmentsAsync(Guid id,
            CancellationToken cancellationToken = default)
        {
            var department = await _context.Divisions.SingleOrDefaultAsync(
                t => t.Id == id && t.TenantId == _currentUserService.TenantId, cancellationToken);

            return department is null
                ? new NotFound()
                : await _context.Departments
                    .Where(sa => sa.DivisionId == id)
                    .ProjectToType<DivisionDepartmentDto>()
                    .ToArrayAsync(cancellationToken);
        }

        public async Task<OneOf<QueryablePaging<DivisionUserDto>, NotFound>> GetDivisionUsersAsync(Guid divisionId,
            GridifyQuery gridifyQuery, CancellationToken cancellationToken = default)
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
                .Select(u => new DivisionUserDto()
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Department = u.Department == null ? string.Empty : u.Department.Name,
                    JobTitle = u.JobTitle == null ? string.Empty : u.JobTitle.Name,
                })
                .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

            return users;
        }
    }
}

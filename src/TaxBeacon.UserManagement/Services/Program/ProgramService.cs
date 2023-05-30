using Gridify;
using Gridify.EntityFramework;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using System.Linq.Expressions;
using System.Text.Json;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services.Program.Activities;
using TaxBeacon.UserManagement.Services.Program.Activities.Models;
using TaxBeacon.UserManagement.Services.Program.Models;

namespace TaxBeacon.UserManagement.Services.Program;

public class ProgramService: IProgramService
{
    private readonly ILogger<ProgramService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    private readonly IDateTimeFormatter _dateTimeFormatter;
    private readonly IImmutableDictionary<(ProgramEventType, uint), IProgramActivityFactory> _activityFactories;

    public ProgramService(
        ILogger<ProgramService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IDateTimeFormatter dateTimeFormatter,
        IEnumerable<IProgramActivityFactory> programActivityFactories)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _dateTimeFormatter = dateTimeFormatter;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
        _activityFactories = programActivityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                             ?? ImmutableDictionary<(ProgramEventType, uint), IProgramActivityFactory>.Empty;
    }

    public Task<QueryablePaging<ProgramDto>> GetAllProgramsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        _context.Programs
            .ProjectToType<ProgramDto>()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

    public async Task<OneOf<ProgramDetailsDto, NameAlreadyExists>> CreateProgramAsync(CreateProgramDto createProgramDto,
        CancellationToken cancellationToken = default)
    {
        if (await _context.Programs.AnyAsync(p => p.Name == createProgramDto.Name, cancellationToken))
        {
            return new NameAlreadyExists();
        }

        var newProgram = createProgramDto.Adapt<DAL.Entities.Program>();
        await _context.Programs.AddAsync(newProgram, cancellationToken);

        var (userFullName, userRoles) = _currentUserService.UserInfo;
        var eventDateTime = _dateTimeService.UtcNow;

        await _context.ProgramActivityLogs.AddAsync(
            new ProgramActivityLog
            {
                TenantId = null,
                Program = newProgram,
                Date = eventDateTime,
                Revision = 1,
                EventType = ProgramEventType.ProgramCreatedEvent,
                Event = JsonSerializer.Serialize(new ProgramCreatedEvent(
                    _currentUserService.UserId,
                    eventDateTime,
                    userFullName,
                    userRoles))
            }, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Program ({programId} was created by {@userId}",
            eventDateTime,
            newProgram.Id,
            _currentUserService.UserId);

        return newProgram.Adapt<ProgramDetailsDto>();
    }

    public async Task<byte[]> ExportProgramsAsync(FileType fileType, CancellationToken cancellationToken = default)
    {
        var exportPrograms = await _context
            .Programs
            .ProjectToType<ProgramExportModel>()
            .ToListAsync(cancellationToken);

        exportPrograms.ForEach(p =>
        {
            p.StartDateView = _dateTimeFormatter.FormatDate(p.StartDateTimeUtc);
            p.EndDateView = _dateTimeFormatter.FormatDate(p.EndDateTimeUtc);
            p.CreatedDateView = _dateTimeFormatter.FormatDate(p.CreatedDateTimeUtc);
        });

        _logger.LogInformation("{dateTime} - Programs export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportPrograms);
    }

    public async Task<OneOf<ProgramDetailsDto, NotFound>> GetProgramDetailsAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var program = await _context.Programs
            .Where(p => p.Id == id)
            .ProjectToType<ProgramDetailsDto>()
            .SingleOrDefaultAsync(cancellationToken);

        return program is not null ? program : new NotFound();
    }

    public async Task<OneOf<ActivityDto, NotFound>> GetProgramActivityHistoryAsync(
        Guid id, int page = 1, int pageSize = 10, CancellationToken cancellationToken = default)
    {
        var program = await GetProgramByIdAsync(id, cancellationToken);

        if (program is null)
        {
            return new NotFound();
        }

        var activityLogs = _currentUserService is { IsSuperAdmin: true, IsUserInTenant: false }
            ? GetProgramActivityLogsQuery(program.Id)
            : GetTenantProgramActivityLogsQuery(program.Id, _currentUserService.TenantId);

        var count = await activityLogs.CountAsync(cancellationToken);

        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await activityLogs
            .OrderByDescending(log => log.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new ActivityDto(pageCount,
            activities.Select(x => _activityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }

    public async Task<OneOf<ProgramDetailsDto, NotFound, NameAlreadyExists>> UpdateProgramAsync(Guid id,
        UpdateProgramDto updateProgramDto,
        CancellationToken cancellationToken = default)
    {
        if (await _context.Programs.AnyAsync(p => p.Name == updateProgramDto.Name && p.Id != id, cancellationToken))
        {
            return new NameAlreadyExists();
        }

        var program = await _context.Programs.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (program is null)
        {
            return new NotFound();
        }

        var (userFullName, userRoles) = _currentUserService.UserInfo;
        var previousValues = JsonSerializer.Serialize(program.Adapt<UpdateProgramDto>());

        var eventDateTime = _dateTimeService.UtcNow;

        await _context.ProgramActivityLogs.AddAsync(new ProgramActivityLog
        {
            ProgramId = program.Id,
            Date = eventDateTime,
            Revision = 1,
            EventType = ProgramEventType.ProgramUpdatedEvent,
            Event = JsonSerializer.Serialize(new ProgramUpdatedEvent(
                _currentUserService.UserId,
                userFullName,
                userRoles,
                eventDateTime,
                previousValues,
                JsonSerializer.Serialize(updateProgramDto)))
        }, cancellationToken);

        updateProgramDto.Adapt(program);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Program ({program}) was updated by {@userId}",
            eventDateTime,
            id,
            _currentUserService.UserId);

        return program.Adapt<ProgramDetailsDto>();
    }

    public async Task<OneOf<TenantProgramDetailsDto, NotFound>> UpdateTenantProgramStatusAsync(Guid id, Status status,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId;

        // TODO: Move the same code into separated method
        var tenantProgram = await _context
            .TenantsPrograms
            .FirstOrDefaultAsync(p => p.ProgramId == id && p.TenantId == tenantId, cancellationToken);

        if (tenantProgram is null)
        {
            return new NotFound();
        }

        var now = _dateTimeService.UtcNow;

        switch (status)
        {
            case Status.Deactivated:
                tenantProgram.DeactivationDateTimeUtc = now;
                tenantProgram.ReactivationDateTimeUtc = null;
                tenantProgram.Status = Status.Deactivated;
                break;
            case Status.Active:
                tenantProgram.ReactivationDateTimeUtc = now;
                tenantProgram.DeactivationDateTimeUtc = null;
                tenantProgram.Status = Status.Active;
                break;
        }

        tenantProgram.Status = status;

        var (currentUserFullName, currentUserRoles) = _currentUserService.UserInfo;

        var programActivityLog = status switch
        {
            Status.Active => new ProgramActivityLog
            {
                TenantId = tenantId,
                ProgramId = tenantProgram.ProgramId,
                Date = now,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ProgramReactivatedEvent(_currentUserService.UserId,
                        now,
                        currentUserFullName,
                        currentUserRoles
                    )),
                EventType = ProgramEventType.ProgramReactivatedEvent
            },
            Status.Deactivated => new ProgramActivityLog
            {
                TenantId = tenantId,
                ProgramId = tenantProgram.ProgramId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new ProgramDeactivatedEvent(_currentUserService.UserId,
                        now,
                        currentUserFullName,
                        currentUserRoles)),
                EventType = ProgramEventType.ProgramDeactivatedEvent
            },
            _ => throw new InvalidOperationException()
        };

        await _context.ProgramActivityLogs.AddAsync(programActivityLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "{dateTime} - Program ({createdProgramId}) status was changed to {newUserStatus} by {@userId}",
            _dateTimeService.UtcNow,
            tenantProgram.ProgramId,
            status,
            _currentUserService.UserId);

        return await GetTenantProgramDetailsAsync(id, cancellationToken);
    }

    public Task<QueryablePaging<TenantProgramDto>> GetAllTenantProgramsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        _context.TenantsPrograms
            .Where(x => x.TenantId == _currentUserService.TenantId)
            .ProjectToType<TenantProgramDto>()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

    public async Task<byte[]> ExportTenantProgramsAsync(FileType fileType,
        CancellationToken cancellationToken = default)
    {
        var exportPrograms = await _context
            .TenantsPrograms
            .Where(x => x.TenantId == _currentUserService.TenantId)
            .ProjectToType<TenantProgramExportModel>()
            .ToListAsync(cancellationToken);

        exportPrograms.ForEach(p =>
        {
            p.StartDateView = _dateTimeFormatter.FormatDate(p.StartDateTimeUtc);
            p.EndDateView = _dateTimeFormatter.FormatDate(p.EndDateTimeUtc);
            p.CreatedDateView = _dateTimeFormatter.FormatDate(p.CreatedDateTimeUtc);
        });

        _logger.LogInformation("{dateTime} - Tenant programs export was executed by {@userId}",
            _dateTimeService.UtcNow,
            _currentUserService.UserId);

        return _listToFileConverters[fileType].Convert(exportPrograms);
    }

    public async Task<OneOf<TenantProgramDetailsDto, NotFound>> GetTenantProgramDetailsAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var program = await _context.TenantsPrograms
            .Where(p => p.ProgramId == id && p.TenantId == _currentUserService.TenantId)
            .ProjectToType<TenantProgramDetailsDto>()
            .FirstOrDefaultAsync(cancellationToken);

        return program is not null ? program : new NotFound();
    }

    private IQueryable<ProgramActivityLog> GetProgramActivityLogsQuery(Guid programId) =>
        _context.ProgramActivityLogs
            .Where(log => log.ProgramId == programId && log.TenantId == null);

    private IQueryable<ProgramActivityLog> GetTenantProgramActivityLogsQuery(Guid programId, Guid tenantId) =>
        _context.ProgramActivityLogs
            .Where(log => log.ProgramId == programId && log.TenantId == tenantId);

    private Task<DAL.Entities.Program?> GetProgramByIdAsync(Guid programId, CancellationToken cancellationToken)
    {
        Expression<Func<DAL.Entities.Program, bool>> predicate =
            _currentUserService is { IsSuperAdmin: true, IsUserInTenant: false }
                ? program => program.Id == programId
                : program => program.Id == programId &&
                             program.TenantsPrograms.Any(tp => tp.TenantId == _currentUserService.TenantId);

        return _context.Programs.FirstOrDefaultAsync(predicate, cancellationToken);
    }
}

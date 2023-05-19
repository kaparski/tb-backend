using Gridify;
using Gridify.EntityFramework;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Programs;

namespace TaxBeacon.UserManagement.Services;

public class ProgramService: IProgramService
{
    private readonly ILogger<ProgramService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImmutableDictionary<FileType, IListToFileConverter> _listToFileConverters;
    private readonly IDateTimeFormatter _dateTimeFormatter;

    public ProgramService(
        ILogger<ProgramService> logger,
        ITaxBeaconDbContext context,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService,
        IEnumerable<IListToFileConverter> listToFileConverters,
        IDateTimeFormatter dateTimeFormatter)
    {
        _logger = logger;
        _context = context;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
        _dateTimeFormatter = dateTimeFormatter;
        _listToFileConverters = listToFileConverters?.ToImmutableDictionary(x => x.FileType)
                                ?? ImmutableDictionary<FileType, IListToFileConverter>.Empty;
    }

    public Task<QueryablePaging<ProgramDto>> GetAllProgramsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        _context.Programs
            .AsNoTracking()
            .Select(p => new ProgramDto
            {
                Id = p.Id,
                Name = p.Name,
                Reference = p.Reference ?? string.Empty,
                Overview = p.Overview ?? string.Empty,
                LegalAuthority = p.LegalAuthority ?? string.Empty,
                Agency = p.Agency ?? string.Empty,
                Jurisdiction = p.Jurisdiction,
                JurisdictionName = p.JurisdictionName ?? string.Empty,
                IncentivesArea = p.IncentivesArea ?? string.Empty,
                IncentivesType = p.IncentivesType ?? string.Empty,
                StartDateTimeUtc = p.StartDateTimeUtc,
                EndDateTimeUtc = p.EndDateTimeUtc,
                CreatedDateTimeUtc = p.CreatedDateTimeUtc
            })
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

    public async Task<byte[]> ExportProgramsAsync(FileType fileType, CancellationToken cancellationToken = default)
    {
        var exportPrograms = await _context
            .Programs
            .AsNoTracking()
            .Select(p => new ProgramExportModel
            {
                Name = p.Name,
                Reference = p.Reference ?? string.Empty,
                Overview = p.Overview ?? string.Empty,
                LegalAuthority = p.LegalAuthority ?? string.Empty,
                Agency = p.Agency ?? string.Empty,
                Jurisdiction = p.Jurisdiction.ToString(),
                JurisdictionName = p.JurisdictionName ?? string.Empty,
                IncentivesArea = p.IncentivesArea ?? string.Empty,
                IncentivesType = p.IncentivesType ?? string.Empty,
                StartDateTimeUtc = p.StartDateTimeUtc,
                EndDateTimeUtc = p.EndDateTimeUtc,
                CreatedDateTimeUtc = p.CreatedDateTimeUtc
            })
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
        var program = await _context.Programs.SingleOrDefaultAsync(p => p.Id == id, cancellationToken);

        return program is not null ? program.Adapt<ProgramDetailsDto>() : new NotFound();
    }

    public Task<OneOf<ActivityDto, NotFound>> GetProgramActivityHistory(Guid id, int page, int pageSize,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public Task<OneOf<ProgramDetailsDto, NotFound>> UpdateProgramAsync(Guid id, UpdateProgramDto updateTenantDto,
        CancellationToken cancellationToken = default) => throw new NotImplementedException();

    public async Task<TenantProgramDto> UpdateTenantProgramStatusAsync(Guid id, Status status,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _currentUserService.TenantId;

        // TODO: Move the same code into separated method
        var program = await _context
                       .Programs
                       .FirstOrDefaultAsync(p => p.Id == id, cancellationToken)
                   ?? throw new NotFoundException(nameof(User), id);

        switch (status)
        {
            case Status.Deactivated:
                program.DeactivationDateTimeUtc = _dateTimeService.UtcNow;
                program.ReactivationDateTimeUtc = null;
                //  program.Status = Status.Deactivated;
                break;
            case Status.Active:
                program.ReactivationDateTimeUtc = _dateTimeService.UtcNow;
                program.DeactivationDateTimeUtc = null;
                // program.Status = Status.Active;
                break;
        }

        // program.Status = status;

        var now = _dateTimeService.UtcNow;
        var (currentUserFullName, currentUserRoles) = _currentUserService.UserInfo;

        //var userActivityLog = status switch
        //{
        //    Status.Active => new UserActivityLog
        //    {
        //        TenantId = tenantId,
        //        ProgramId = tenantProgram.ProgramId,
        //        Date = now,
        //        Revision = 1,
        //        Event = JsonSerializer.Serialize(
        //            new UserReactivatedEvent(_currentUserService.UserId,
        //                now,
        //                currentUserFullName,
        //                currentUserRoles
        //                )),
        //        EventType = UserEventType.UserReactivated
        //    },
        //    Status.Deactivated => new UserActivityLog
        //    {
        //        TenantId = tenantId,
        //        ProgramId = tenantProgram.ProgramId,
        //        Date = _dateTimeService.UtcNow,
        //        Revision = 1,
        //        Event = JsonSerializer.Serialize(
        //            new UserDeactivatedEvent(_currentUserService.UserId,
        //                now,
        //                currentUserFullName,
        //                currentUserRoles)),
        //        EventType = UserEventType.UserDeactivated
        //    },
        //    _ => throw new InvalidOperationException()
        //};

        //await _context.UserActivityLogs.AddAsync(userActivityLog, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Program ({createdProgramId}) status was changed to {newUserStatus} by {@userId}",
            _dateTimeService.UtcNow,
            program.Id,
            status,
            _currentUserService.UserId);

        return program.Adapt<TenantProgramDto>();
    }

    public Task<QueryablePaging<TenantProgramDto>> GetAllTenantProgramsAsync(GridifyQuery gridifyQuery, CancellationToken cancellationToken = default)
        =>
            _context.TenantsPrograms
                .AsNoTracking()
                .Where(x => x.TenantId == _currentUserService.TenantId)
                .Select(p => new TenantProgramDto
                {
                    Id = p.ProgramId,
                    Name = p.Program.Name,
                    Reference = p.Program.Reference ?? string.Empty,
                    Overview = p.Program.Overview ?? string.Empty,
                    LegalAuthority = p.Program.LegalAuthority ?? string.Empty,
                    Agency = p.Program.Agency ?? string.Empty,
                    Jurisdiction = p.Program.Jurisdiction,
                    JurisdictionName = p.Program.JurisdictionName ?? string.Empty,
                    IncentivesArea = p.Program.IncentivesArea ?? string.Empty,
                    IncentivesType = p.Program.IncentivesType ?? string.Empty,
                    Department = string.Empty,
                    ServiceArea = string.Empty,
                    StartDateTimeUtc = p.Program.StartDateTimeUtc,
                    EndDateTimeUtc = p.Program.EndDateTimeUtc,
                    CreatedDateTimeUtc = p.Program.CreatedDateTimeUtc,
                    Status = p.Status
                })
                .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

    public async Task<byte[]> ExportTenantProgramsAsync(FileType fileType, CancellationToken cancellationToken = default)
    {
        var exportPrograms = await _context
            .TenantsPrograms
            .Where(x => x.TenantId == _currentUserService.TenantId)
            .AsNoTracking()
            .Select(p => new TenantProgramExportModel()
            {
                Name = p.Program.Name,
                Reference = p.Program.Reference ?? string.Empty,
                Overview = p.Program.Overview ?? string.Empty,
                LegalAuthority = p.Program.LegalAuthority ?? string.Empty,
                Agency = p.Program.Agency ?? string.Empty,
                Jurisdiction = p.Program.Jurisdiction.ToString(),
                JurisdictionName = p.Program.JurisdictionName ?? string.Empty,
                IncentivesArea = p.Program.IncentivesArea ?? string.Empty,
                IncentivesType = p.Program.IncentivesType ?? string.Empty,
                StartDateTimeUtc = p.Program.StartDateTimeUtc,
                EndDateTimeUtc = p.Program.EndDateTimeUtc,
                CreatedDateTimeUtc = p.Program.CreatedDateTimeUtc,
                Status = p.Status,
            })
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
            .Include(x => x.Program)
            .FirstOrDefaultAsync(p => p.ProgramId == id && p.TenantId == _currentUserService.TenantId, cancellationToken);

        if (program is null)
        {
            return new NotFound();
        }

        var programDetailsDto = program.Program.Adapt<TenantProgramDetailsDto>();
        programDetailsDto.Status = program.Status;
        programDetailsDto.Jurisdiction = program.Program.Jurisdiction.ToString();

        return programDetailsDto;
    }
}

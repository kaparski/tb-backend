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
}
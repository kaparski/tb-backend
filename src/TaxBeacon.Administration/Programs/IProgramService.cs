using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Administration.Programs.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Programs;

public interface IProgramService
{
    Task<OneOf<ProgramDetailsDto, NameAlreadyExists>> CreateProgramAsync(CreateProgramDto createProgramDto, CancellationToken cancellationToken);

    Task<byte[]> ExportProgramsAsync(FileType fileType, CancellationToken cancellationToken = default);

    Task<byte[]> ExportTenantProgramsAsync(FileType fileType, CancellationToken cancellationToken = default);

    Task<QueryablePaging<ProgramDto>> GetAllProgramsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<QueryablePaging<TenantProgramDto>> GetAllTenantProgramsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetProgramActivityHistoryAsync(Guid id,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<OneOf<ProgramDetailsDto, NotFound>> GetProgramDetailsAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<OneOf<TenantProgramDetailsDto, NotFound>> GetTenantProgramDetailsAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<OneOf<ProgramDetailsDto, NotFound, NameAlreadyExists>> UpdateProgramAsync(Guid id,
        UpdateProgramDto updateProgramDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<TenantProgramDetailsDto, NotFound>> UpdateTenantProgramStatusAsync(Guid id,
        Status status,
        CancellationToken cancellationToken = default);

    Task<OneOf<TenantProgramOrgUnitsAssignmentDto, NotFound>> ChangeTenantProgramAssignmentAsync(Guid programId,
        AssignTenantProgramDto assignTenantProgramDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<TenantProgramOrgUnitsAssignmentDto, NotFound>> GetTenantProgramOrgUnitsAssignmentAsync(Guid programId,
        CancellationToken cancellationToken = default);

    IQueryable<ProgramDto> QueryPrograms();

    IQueryable<TenantProgramDto> QueryTenantPrograms();
}

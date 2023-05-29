using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services.Program.Models;

namespace TaxBeacon.UserManagement.Services.Program;

public interface IProgramService
{
    Task<QueryablePaging<ProgramDto>> GetAllProgramsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportProgramsAsync(FileType fileType, CancellationToken cancellationToken = default);

    Task<OneOf<ProgramDetailsDto, NotFound>> GetProgramDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetProgramActivityHistoryAsync(Guid id, int page = 1, int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<OneOf<ProgramDetailsDto, NotFound>> UpdateProgramAsync(Guid id, UpdateProgramDto updateProgramDto,
        CancellationToken cancellationToken = default);

    Task<QueryablePaging<TenantProgramDto>> GetAllTenantProgramsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportTenantProgramsAsync(FileType fileType, CancellationToken cancellationToken = default);

    Task<OneOf<TenantProgramDetailsDto, NotFound>> GetTenantProgramDetailsAsync(Guid id,
        CancellationToken cancellationToken = default);

    Task<OneOf<TenantProgramDetailsDto, NotFound>> UpdateTenantProgramStatusAsync(Guid id, Status status,
        CancellationToken cancellationToken = default);
}

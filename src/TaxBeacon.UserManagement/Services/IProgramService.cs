using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Programs;

namespace TaxBeacon.UserManagement.Services;

public interface IProgramService
{
    Task<QueryablePaging<ProgramDto>> GetAllProgramsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportProgramsAsync(FileType fileType, CancellationToken cancellationToken = default);

    Task<OneOf<ProgramDto, NotFound>> GetProgramDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetProgramActivityHistory(Guid id, int page, int pageSize,
        CancellationToken cancellationToken = default);

    Task<OneOf<ProgramDto, NotFound>> UpdateProgramAsync(Guid id, UpdateProgramDto updateTenantDto,
        CancellationToken cancellationToken = default);
}

using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IJobTitleService
{
    IQueryable<JobTitleDto> QueryJobTitles();

    Task<QueryablePaging<JobTitleDto>> GetJobTitlesAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken);

    Task<byte[]> ExportJobTitlesAsync(FileType fileType,
        CancellationToken cancellationToken);

    Task<OneOf<JobTitleDetailsDto, NotFound>> GetJobTitleDetailsByIdAsync(Guid id,
        CancellationToken cancellationToken);

    Task<OneOf<JobTitleDetailsDto, NotFound>> UpdateJobTitleDetailsAsync(Guid id,
        UpdateJobTitleDto updateJobTitleDto,
        CancellationToken cancellationToken);

    Task<OneOf<ActivityDto, NotFound>> GetActivityHistoryAsync(Guid id,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<OneOf<QueryablePaging<JobTitleUserDto>, NotFound>> GetUsersAsync(Guid serviceAreaId,
        GridifyQuery gridifyQuery,
        CancellationToken cancellationToken);
}

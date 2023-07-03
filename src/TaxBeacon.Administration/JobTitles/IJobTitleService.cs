using OneOf;
using OneOf.Types;
using TaxBeacon.Administration.JobTitles.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.JobTitles;

public interface IJobTitleService
{
    IQueryable<JobTitleDto> QueryJobTitles();

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

    Task<IQueryable<JobTitleUserDto>> QueryUsersAsync(Guid jobTitleId);
}

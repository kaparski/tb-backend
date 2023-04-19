using Gridify;
using OneOf.Types;
using OneOf;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models.Activities.DivisionsActivities;

namespace TaxBeacon.UserManagement.Services
{
    public interface ITenantDivisionsService
    {
        Task<OneOf<QueryablePaging<DivisionDto>, NotFound>> GetTenantDivisionsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

        Task<byte[]> ExportTenantDivisionsAsync(FileType fileType,
        CancellationToken cancellationToken);

        Task<OneOf<DivisionActivityDto, NotFound>> GetActivitiesAsync(Guid divisionId, uint page = 1,
            uint pageSize = 10, CancellationToken cancellationToken = default);

        Task<OneOf<DivisionDetailsDto, NotFound>> GetDivisionDetails(Guid divisionId, CancellationToken cancellationToken = default);
    }
}

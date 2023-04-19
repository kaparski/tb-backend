using Gridify;
using OneOf.Types;
using OneOf;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Services
{
    public interface ITenantDivisionsService
    {
        Task<OneOf<QueryablePaging<DivisionDto>, NotFound>> GetTenantDivisionsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

        Task<byte[]> ExportTenantDivisionsAsync(FileType fileType,
        CancellationToken cancellationToken);
    }
}

using Gridify;
using OneOf.Types;
using OneOf;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Services
{
    public interface IDivisionsService
    {
        Task<OneOf<QueryablePaging<DivisionDto>, NotFound>> GetDivisionsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

        Task<byte[]> ExportDivisionsAsync(FileType fileType,
        CancellationToken cancellationToken);
    }
}

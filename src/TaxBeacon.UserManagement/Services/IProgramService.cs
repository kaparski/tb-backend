using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IProgramService
{
    public Task<OneOf<QueryablePaging<TenantProgramDto>, NotFound>> GetProgramsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);
}

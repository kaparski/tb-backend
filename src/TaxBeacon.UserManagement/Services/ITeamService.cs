using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface ITeamService
{
    public Task<OneOf<QueryablePaging<TeamDto>, NotFound>> GetTeamsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportTeamsAsync(FileType fileType, CancellationToken cancellationToken);
}

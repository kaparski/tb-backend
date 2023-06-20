using Gridify;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.Common.Models;

namespace TaxBeacon.UserManagement.Services;

public interface ITeamService
{
    IQueryable<TeamDto> QueryTeams();

    Task<QueryablePaging<TeamDto>> GetTeamsAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task<byte[]> ExportTeamsAsync(FileType fileType, CancellationToken cancellationToken = default);

    Task<OneOf<TeamDto, NotFound>> UpdateTeamAsync(Guid id, UpdateTeamDto updateTeamDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<TeamDetailsDto, NotFound>> GetTeamDetailsAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid teamId, int page = 1,
        int pageSize = 10, CancellationToken cancellationToken = default);

    Task<OneOf<QueryablePaging<TeamUserDto>, NotFound>> GetTeamUsersAsync(Guid teamId, GridifyQuery gridifyQuery, CancellationToken cancellationToken = default);

    Task<IQueryable<TeamUserDto>> QueryTeamUsersAsync(Guid teamId);
}

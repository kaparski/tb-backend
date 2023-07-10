using OneOf;
using OneOf.Types;
using TaxBeacon.Administration.Teams.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Teams;

public interface ITeamService
{
    IQueryable<TeamDto> QueryTeams();

    Task<byte[]> ExportTeamsAsync(FileType fileType, CancellationToken cancellationToken = default);

    Task<OneOf<TeamDto, NotFound>> UpdateTeamAsync(Guid id, UpdateTeamDto updateTeamDto,
        CancellationToken cancellationToken = default);

    Task<OneOf<TeamDetailsDto, NotFound>> GetTeamDetailsAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<OneOf<ActivityDto, NotFound>> GetActivitiesAsync(Guid teamId, int page = 1,
        int pageSize = 10, CancellationToken cancellationToken = default);

    Task<IQueryable<TeamUserDto>> QueryTeamUsersAsync(Guid teamId);
}

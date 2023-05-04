using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Teams.Requests;
using TaxBeacon.API.Controllers.Teams.Responses;
using TaxBeacon.API.Controllers.Tenants.Requests;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Teams;

[Authorize]
public class TeamsController: BaseController
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService) => _teamService = teamService;

    /// <summary>
    /// Get teams for Table
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /teams?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DPeter```<br/><br/>
    ///     ```GET /teams?page=2&amp;pageSize=5&amp;orderBy=name```
    /// </remarks>
    /// <response code="200">Returns teams</response>
    /// <returns>List of teams</returns>
    [HasPermissions(
        Common.Permissions.Teams.Read,
        Common.Permissions.Teams.ReadExport,
        Common.Permissions.Teams.ReadWrite)]
    [HttpGet(Name = "GetTeams")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<TeamResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequest), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamList([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<TeamDto>())
        {
            return BadRequest();
        }

        var teamOneOf = await _teamService.GetTeamsAsync(query, cancellationToken);
        return teamOneOf.Match<IActionResult>(
            teams => Ok(new QueryablePaging<TeamResponse>(teams.Count, teams.Query.ProjectToType<TeamResponse>())),
            notFound => NotFound());
    }

    /// <summary>
    /// Endpoint to export teams
    /// </summary>
    /// <param name="exportTeamsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Teams.ReadExport)]
    [HttpGet("export", Name = "ExportTeams")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportTeamsAsync([FromQuery] ExportTeamsRequest exportTeamsRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportTeamsRequest.FileType.ToMimeType();

        var teams = await _teamService.ExportTeamsAsync(exportTeamsRequest.FileType, cancellationToken);

        return File(teams, mimeType, $"teams.{exportTeamsRequest.FileType.ToString().ToLowerInvariant()}");
    }

    /// <summary>
    /// Get teams Activity History
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="404">Team is not found</response>
    [HasPermissions(Common.Permissions.Teams.Read, Common.Permissions.Teams.ReadWrite)]
    [HttpGet("{id:guid}/activities", Name = "TeamActivityHistory")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<TeamActivityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TeamActivitiesHistory([FromRoute] Guid id, [FromQuery] TeamActivityRequest request,
        CancellationToken cancellationToken)
    {
        var activities = await _teamService.GetActivitiesAsync(id, request.Page, request.PageSize, cancellationToken);

        return activities.Match<IActionResult>(
            result => Ok(result.Adapt<TeamActivityResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Get Team By Id
    /// </summary>
    /// <response code="200">Returns Team Details</response>
    /// <response code="404">Team is not found</response>
    [HasPermissions(Common.Permissions.Teams.Read, Common.Permissions.Teams.ReadWrite)]
    [HttpGet("{id:guid}", Name = "TeamDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<TeamDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeamDetails([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var oneOfTeamDetails = await _teamService.GetTeamDetailsAsync(id, cancellationToken);

        return oneOfTeamDetails.Match<IActionResult>(
            result => Ok(result.Adapt<TeamDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Update team details
    /// </summary>
    /// <response code="200">Returns updated team</response>
    /// <response code="404">Team is not found</response>
    /// <returns>Updated team</returns>
    [HasPermissions(Common.Permissions.Teams.ReadWrite)]
    [HttpPatch("{id:guid}", Name = "UpdateTeam")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTeamAsync([FromRoute] Guid id, [FromBody] UpdateTeamRequest request, CancellationToken cancellationToken)
    {
        var resultOneOf = await _teamService.UpdateTeamAsync(id, request.Adapt<UpdateTeamDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<TeamResponse>()),
            notFound => NotFound());
    }
}

using FluentValidation.Results;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Teams.Requests;
using TaxBeacon.API.Controllers.Teams.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.Administration.Teams;
using TaxBeacon.Administration.Teams.Models;

namespace TaxBeacon.API.Controllers.Teams;

[Authorize]
public class TeamsController: BaseController
{
    private readonly ITeamService _teamService;

    public TeamsController(ITeamService teamService) => _teamService = teamService;

    /// <summary>
    /// Queryable list of teams
    /// </summary>
    /// <response code="200">Returns teams</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of teams</returns>
    [HasPermissions(
        Common.Permissions.Teams.Read,
        Common.Permissions.Teams.ReadExport,
        Common.Permissions.Teams.ReadWrite)]
    [EnableQuery]
    [HttpGet("api/odata/teams")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<TeamResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<TeamResponse> Get()
    {
        var query = _teamService.QueryTeams();

        return query.ProjectToType<TeamResponse>();
    }

    /// <summary>
    /// Endpoint to export teams
    /// </summary>
    /// <param name="exportTeamsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Teams.ReadExport)]
    [HttpGet("export", Name = "ExportTeams")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Team is not found</response>
    /// <returns>Activity history for a specific team</returns>
    [HasPermissions(Common.Permissions.Teams.Read, Common.Permissions.Teams.ReadWrite)]
    [HttpGet("{id:guid}/activities", Name = "TeamActivityHistory")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<TeamActivityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Team is not found</response>
    /// <returns>Team details</returns>
    [HasPermissions(Common.Permissions.Teams.Read, Common.Permissions.Teams.ReadWrite)]
    [HttpGet("{id:guid}", Name = "TeamDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<TeamDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
    /// <response code="400">Validation failed</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Team is not found</response>
    /// <returns>Updated team</returns>
    [HasPermissions(Common.Permissions.Teams.ReadWrite)]
    [HttpPatch("{id:guid}", Name = "UpdateTeam")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(TeamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTeamAsync([FromRoute] Guid id, [FromBody] UpdateTeamRequest request,
        CancellationToken cancellationToken)
    {
        var resultOneOf = await _teamService.UpdateTeamAsync(id, request.Adapt<UpdateTeamDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<TeamResponse>()),
            notFound => NotFound());
    }
}

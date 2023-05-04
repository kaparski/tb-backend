﻿using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Teams.Requests;
using TaxBeacon.API.Controllers.Teams.Responses;
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
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of teams</returns>
    [HasPermissions(
        Common.Permissions.Teams.Read,
        Common.Permissions.Teams.ReadExport,
        Common.Permissions.Teams.ReadWrite)]
    [HttpGet(Name = "GetTeams")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<TeamResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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
}

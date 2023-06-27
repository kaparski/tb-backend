using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Teams.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.UserManagement.Teams;

namespace TaxBeacon.API.Controllers.Teams;

[Authorize]
public class TeamUsersController: BaseController
{
    private readonly ITeamService _service;

    public TeamUsersController(ITeamService teamService) => _service = teamService;

    /// <summary>
    /// Queryable list of users in a given team
    /// </summary>
    /// <response code="200">Returns Team Users</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">User does not have the required permission</response>
    /// <response code="404">Team is not found</response>
    /// <returns>A collection of users assigned to a particular team</returns>
    [HasPermissions(Common.Permissions.Teams.Read, Common.Permissions.Teams.ReadWrite)]
    [EnableQuery]
    [HttpGet("api/odata/teams/{id:guid}/teamusers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<TeamUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        try
        {
            var query = await _service.QueryTeamUsersAsync(id);

            return Ok(query.ProjectToType<TeamUserResponse>());
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}

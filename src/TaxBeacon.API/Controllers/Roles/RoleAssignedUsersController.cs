using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Roles;

/// <summary>
/// Had to move this method to a separate controller, to make OData routing work.
/// TODO: find a better way to handle OData routing.
/// </summary>
[Authorize]
public class RoleAssignedUsersController: BaseController
{
    private readonly IRoleService _roleService;

    public RoleAssignedUsersController(IRoleService roleService) => _roleService = roleService;

    /// <summary>
    /// Queryable list of users of a given role
    /// </summary>
    /// <response code="200">Returns list of role assigned users</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">The role was not found</response>
    /// <returns>A collection of users assigned to a particular role</returns>
    [HasPermissions(Common.Permissions.Roles.Read)]
    [EnableQuery]
    [HttpGet("api/odata/roles/{id:guid}/roleassignedusers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<RoleAssignedUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        try
        {
            var query = await _roleService.QueryRoleAssignedUsersAsync(id);

            return Ok(query.ProjectToType<RoleAssignedUserResponse>());
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    /// <summary>
    /// Unassign users from specific role
    /// </summary>
    /// <remarks>
    /// Permission: Roles.ReadWrite
    /// </remarks>
    /// <response code="204">Users have been successfully unassigned</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">The role was not found</response>
    /// <returns>No content response</returns>
    [HasPermissions(Common.Permissions.Roles.ReadWrite)]
    [HttpDelete("/api/odata/roles/{id:guid}/roleassignedusers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassignUsers([FromRoute] Guid id,
        [FromBody] List<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var result = await _roleService.UnassignUsersAsync(id, userIds, cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            _ => NotFound());
    }

    /// <summary>
    /// Endpoint for assigning users to a specific role
    /// </summary>
    /// <param name="id">Role id</param>
    /// <param name="userIds">List of users ids</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <response code="200">Users have been successfully assigned</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">The role was not found</response>
    /// <returns>Success response</returns>
    [HasPermissions(Common.Permissions.Roles.ReadWrite)]
    [HttpPost("/api/odata/roles/{id:guid}/roleassignedusers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignUsersToRole([FromRoute] Guid id,
        [FromBody] List<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var resultOneOf = await _roleService.AssignUsersAsync(id, userIds, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            _ => Ok(),
            _ => NotFound());
    }
}

using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.API.Exceptions;
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
    /// Queryable list of user of a given role
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
    [ProducesResponseType(typeof(QueryablePaging<RoleAssignedUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        var getRoleResult = await _roleService.GetRoleByIdAsync(id);
        if (!getRoleResult.TryPickT0(out var role, out var notFound))
        {
            return NotFound();
        }

        var query = await _roleService.QueryRoleAssignedUsersAsync(id);

        return Ok(query.ProjectToType<RoleAssignedUserResponse>());
    }
}

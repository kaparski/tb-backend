using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Administration.Roles;

namespace TaxBeacon.API.Controllers.Roles;

[Authorize]
public class RolesController: BaseController
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService) => _roleService = roleService;

    /// <summary>
    /// Queryable list of roles
    /// </summary>
    /// <response code="200">Returns roles</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Roles collection</returns>
    [HasPermissions(Common.Permissions.Roles.Read, Common.Permissions.Roles.ReadWrite)]
    [EnableQuery]
    [HttpGet("api/odata/roles")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<RoleResponse> Get()
    {
        var query = _roleService.QueryRoles();

        return query.ProjectToType<RoleResponse>();
    }

    /// <summary>
    /// List of permissions for selected role
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /api/roles/8da4f695-6d47-4ce8-da8f-08db0052f325/permissions```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns role's permissions</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">The role was not found</response>
    /// <returns>A collection of permissions for a specific role</returns>
    [HasPermissions(Common.Permissions.Roles.Read)]
    [HttpGet("{roleId:guid}/permissions", Name = "GetPermissionsByRoleId")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(PermissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPermissionsByRoleId(Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var getPermissionsResult = await _roleService.GetRolePermissionsByIdAsync(roleId, cancellationToken);

        return getPermissionsResult.Match<IActionResult>(
            permissions => Ok(permissions.Adapt<List<PermissionResponse>>()),
            _ => NotFound());
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
    [HttpDelete("{id:guid}/users", Name = "UnassignUsers")]
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
    [HttpPost("{id:guid}/users", Name = "AssignUsersToRole")]
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

    /// <summary>
    /// Get Role By Id
    /// </summary>
    /// <response code="200">Returns Role Details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Role is not found</response>
    /// <returns>Role details</returns>
    [HasPermissions(Common.Permissions.Roles.Read, Common.Permissions.Roles.ReadWrite)]
    [HttpGet("{id:guid}", Name = "GetRoleById")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleByIdAsync([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _roleService.GetRoleByIdAsync(id, cancellationToken);

        return result.Match<IActionResult>(
            role => Ok(role.Adapt<RoleResponse>()),
            _ => NotFound());
    }
}

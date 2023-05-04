﻿using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Roles;

[Authorize]
public class RolesController: BaseController
{
    private readonly IRoleService _roleService;
    private readonly IPermissionsService _permissionService;

    public RolesController(IRoleService roleService, IPermissionsService permissionService)
    {
        _permissionService = permissionService;
        _roleService = roleService;
    }

    /// <summary>
    /// List of roles
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET api/roles?page=1&amp;pageSize=10&amp;orderBy=name%20asc&amp;filter=name%3DAdmin```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns roles</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Roles collection</returns>
    [HasPermissions(Common.Permissions.Roles.Read, Common.Permissions.Roles.ReadWrite)]
    [HttpGet(Name = "GetRoles")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<RoleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<QueryablePaging<RoleResponse>>> GetRoleList([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetRolesAsync(query, cancellationToken);
        var roleListResponse =
            new QueryablePaging<RoleResponse>(roles.Count, roles.Query.ProjectToType<RoleResponse>());

        return Ok(roleListResponse);
    }

    /// <summary>
    /// List of user of a given role
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET api/roles/8da4f695-6d47-4ce8-da8f-08db0052f325/users?page=1&amp;pageSize=10&amp;orderBy=email%20asc&amp;filter=email%3DAdmin```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns list of role assigned users</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>A collection of users assigned to a particular role</returns>
    [HasPermissions(Common.Permissions.Roles.Read)]
    [HttpGet("{id:guid}/users", Name = "GetRoleAssignedUsers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<RoleAssignedUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRoleAssignedUsers([FromRoute] Guid id, [FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        var usersOneOf = await _roleService.GetRoleAssignedUsersAsync(id, query, cancellationToken);

        return usersOneOf.Match<IActionResult>(
            users => Ok(new QueryablePaging<RoleAssignedUserResponse>(users.Count,
                users.Query.ProjectToType<RoleAssignedUserResponse>())),
            notfound => NotFound());
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
    /// <returns>A collection of permissions for a specific role</returns>
    [HasPermissions(Common.Permissions.Roles.Read)]
    [HttpGet("{roleId:guid}/permissions", Name = "GetPermissionsByRoleId")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(PermissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<PermissionResponse>>> GetPermissionsByRoleId(Guid roleId,
        CancellationToken cancellationToken)
    {
        var permissionsListResponse = await _permissionService.GetPermissionsByRoleIdAsync(roleId, cancellationToken);

        return Ok(permissionsListResponse.Adapt<List<PermissionResponse>>());
    }

    /// <summary>
    /// Unassign users from specific role
    /// </summary>
    /// <remarks>
    /// Permission: Roles.ReadWrite
    /// </remarks>
    /// <response code="204">Users have been successfully unasigned</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
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
        CancellationToken cancellationToken)
    {
        var result = await _roleService.UnassignUsersAsync(id, userIds, cancellationToken);

        return result.Match<IActionResult>(
            success => NoContent(),
            notFound => NotFound());
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
    /// <returns>Success response</returns>
    [HasPermissions(Common.Permissions.Roles.ReadWrite)]
    [HttpPost("{id:guid}/users", Name = "AssignUsersToRole")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignUsersToRole([FromRoute] Guid id, [FromBody] List<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var resultOneOf = await _roleService.AssignUsersAsync(id, userIds, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            success => Ok(),
            notFound => NotFound());
    }
}

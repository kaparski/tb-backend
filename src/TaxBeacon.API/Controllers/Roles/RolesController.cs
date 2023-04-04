using Gridify;
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

    public RolesController(IRoleService roleService) => _roleService = roleService;

    /// <summary>
    /// List of roles
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET api/roles?page=1&amp;pageSize=10&amp;orderBy=name%20asc&amp;filter=name%3DAdmin```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns roles</response>
    [HasPermissions(Common.Permissions.Roles.Read)]
    [HttpGet(Name = "GetRoles")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryablePaging<RoleResponse>>> GetRoleList([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        var roles = await _roleService.GetRolesAsync(Guid.Empty, query, cancellationToken);
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
    [HasPermissions(Common.Permissions.Roles.Read)]
    [HttpGet("{id:guid}/users", Name = "GetRoleAssignedUsers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<RoleAssignedUserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryablePaging<RoleAssignedUserResponse>>> GetRoleAssignedUsers([FromRoute] Guid id, [FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        var users = await _roleService.GetRoleAssignedUsersAsync(Guid.Empty, id, query, cancellationToken);

        var response = new QueryablePaging<RoleAssignedUserResponse>(users.Count, users.Query.ProjectToType<RoleAssignedUserResponse>());

        return Ok(response);
    }
}

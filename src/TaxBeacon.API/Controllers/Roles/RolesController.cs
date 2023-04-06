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
    ///     ```GET /roles?page=1&amp;pageSize=10&amp;orderBy=name%20asc&amp;filter=name%3DAdmin```<br/><br/>
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
    /// List of permissions for selected role
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /api/roles/8da4f695-6d47-4ce8-da8f-08db0052f325/permissions```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns role's permissions</response>
    [HasPermissions(Common.Permissions.Roles.Read)]
    [HttpGet("{roleId:guid}/permissions", Name = "GetPermissionsByRoleId")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(PermissionResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PermissionResponse>>> GetPermissionsByRoleId(Guid roleId,
        CancellationToken cancellationToken)
    {
        var permissionsListResponse = await _permissionService.GetPermissionsByRoleIdAsync(Guid.Empty, roleId, cancellationToken);

        return Ok(permissionsListResponse.Adapt<List<PermissionResponse>>());
    }
}

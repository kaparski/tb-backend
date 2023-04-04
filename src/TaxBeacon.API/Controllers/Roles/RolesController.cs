using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Roles;

[Authorize]
public class RolesController: BaseController
{
    private readonly IRoleService _roleService;
    private readonly ICurrentUserService _currentUserService;

    public RolesController(IRoleService roleService, ICurrentUserService currentUserService)
    {
        _roleService = roleService;
        _currentUserService = currentUserService;
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
    /// Unassign Users
    /// </summary>
    /// <remarks>
    /// Permission: Roles.ReadWrite
    /// </remarks>
    /// <response code="200"></response>
    [HasPermissions(Common.Permissions.Roles.ReadWrite)]
    [HttpDelete("{id:guid}/users", Name = "UnassignUsers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassignUsers([FromRoute] Guid id,
        [FromBody] List<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        var currentUserId = _currentUserService.UserId;

        var result = await _roleService.UnassignUsersAsync(id, userIds, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(),
            notFound => NotFound());
    }
}

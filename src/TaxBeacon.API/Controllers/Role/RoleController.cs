using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Role;

[Authorize]
[Route("api/[controller]/{id:guid}")]
public class RoleController: BaseController
{
    private readonly IRoleService _roleService;
    private readonly ICurrentUserService _currentUserService;

    public RoleController(IRoleService roleService, ICurrentUserService currentUserService)
    {
        _roleService = roleService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Unassign Users
    /// </summary>
    /// <response code="200"></response>
    [HasPermissions(Common.Permissions.Roles.UsersWrite)]
    [HttpDelete(Name = "UnassignUsers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassignUsers([FromRoute] Guid id,
        [FromBody] List<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var tenantId = _currentUserService.TenantId;
        var currentUserId = _currentUserService.UserId;

        var result = await _roleService.UnassignUsersAsync(userIds, tenantId, id, currentUserId, cancellationToken);

        if (result.IsT1)
        {
            return NotFound();
        }

        return Ok();
    }
}

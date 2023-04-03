using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Role;

[Authorize]
[Route("api/[controller]/{id:guid}")]
public class RoleController: BaseController
{
    private readonly IRoleService _roleService;

    public RoleController(IRoleService roleService) => _roleService = roleService;

    /// <summary>
    /// Unassign Users
    /// </summary>
    /// <response code="200"></response>
    [HasPermissions(Common.Permissions.Roles.UsersWrite)]
    [HttpPost(Name = "UnassignUsers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QueryablePaging<RoleResponse>>> UnassignUsers([FromRoute] Guid id,
        [FromBody] List<Guid> userIds,
        CancellationToken cancellationToken)
    {
        var tenantId = Guid.Parse(HttpContext!.User!.FindFirst(Claims.TenantId)!.Value);
        var result = await _roleService.UnassignUsers(userIds, tenantId, cancellationToken, id);

        if (result.IsT1)
        {
            return NotFound();
        }

        return Ok();
    }
}

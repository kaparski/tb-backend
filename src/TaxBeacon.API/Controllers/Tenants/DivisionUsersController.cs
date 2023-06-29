using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Administration.Divisions;

namespace TaxBeacon.API.Controllers.Tenants;

[Authorize]
public class DivisionUsersController: BaseController
{
    private readonly IDivisionsService _service;
    public DivisionUsersController(IDivisionsService divisionsService) => _service = divisionsService;

    /// <summary>
    /// Queryable list of users in a given job title
    /// </summary>
    /// <response code="200">Returns Division Users</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Division is not found</response>
    /// <returns>A collection of users assigned to a particular division</returns>
    [HasPermissions(Common.Permissions.Divisions.Read, Common.Permissions.Divisions.ReadWrite)]
    [EnableQuery]
    [HttpGet("api/odata/divisions/{id:guid}/divisionusers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<DivisionUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        try
        {
            var query = await _service.QueryDivisionUsersAsync(id);

            return Ok(query.ProjectToType<DivisionUserResponse>());
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}
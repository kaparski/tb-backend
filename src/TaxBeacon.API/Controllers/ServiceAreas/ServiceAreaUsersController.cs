using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.UserManagement.ServiceAreas;

namespace TaxBeacon.API.Controllers.ServiceAreas;

[Authorize]
public class ServiceAreaUsersController: BaseController
{
    private readonly IServiceAreaService _service;

    public ServiceAreaUsersController(IServiceAreaService serviceAreaService) => _service = serviceAreaService;

    /// <summary>
    /// Queryable list of users in a given service area
    /// </summary>
    /// <response code="200">Returns Users of service area</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Service area is not found</response>
    /// <returns>Users of service area</returns>
    [HasPermissions(Common.Permissions.ServiceAreas.Read, Common.Permissions.ServiceAreas.ReadWrite)]
    [EnableQuery]
    [HttpGet("api/odata/serviceareas/{id:guid}/serviceareausers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<ServiceAreaUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        try
        {
            var query = await _service.QueryUsersAsync(id);

            return Ok(query.ProjectToType<ServiceAreaUserResponse>());
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}

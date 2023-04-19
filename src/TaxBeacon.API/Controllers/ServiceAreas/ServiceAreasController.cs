using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.ServiceAreas;

[Authorize]
public class ServiceAreasController: BaseController
{
    private readonly ITenantService _tenantService;

    public ServiceAreasController(ITenantService tenantService) => _tenantService = tenantService;

    /// <summary>
    /// List of service areas
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /serviceareas?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns all service areas</response>
    /// <returns>List of departments</returns>
    [HasPermissions(
        Common.Permissions.Departments.Read,
        Common.Permissions.Departments.ReadWrite,
        Common.Permissions.Departments.ReadExport)]
    [HttpGet(Name = "GetServiceAreas")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(ServiceAreaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetServiceAreaList(CancellationToken cancellationToken)
    {
        var items = await _tenantService.GetServiceAreasAsync(cancellationToken);

        return Ok(items.Adapt<List<ServiceAreaResponse>>());
    }
}

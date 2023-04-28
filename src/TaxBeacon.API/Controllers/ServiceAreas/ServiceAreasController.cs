using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.ServiceAreas;

[Authorize]
public class ServiceAreasController: BaseController
{
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public ServiceAreasController(ITenantService tenantService, ICurrentUserService currentUserService)
    {
        _tenantService = tenantService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// List of service areas
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /serviceareas?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns all service areas</response>
    /// <returns>List of service areas</returns>
    [HasPermissions(
        Common.Permissions.Departments.Read,
        Common.Permissions.Departments.ReadWrite,
        Common.Permissions.Departments.ReadExport,
        Common.Permissions.ServiceAreas.Read,
        Common.Permissions.ServiceAreas.ReadWrite,
        Common.Permissions.ServiceAreas.ReadExport)]
    [HttpGet(Name = "GetServiceAreas")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(ServiceAreaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceAreaList([FromQuery] GridifyQuery query, CancellationToken cancellationToken)
    {
        if (!query.IsValid<ServiceAreaDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var tenantId = _currentUserService.TenantId;
        var serviceAreasOneOf = await _tenantService.GetServiceAreasAsync(tenantId, query, cancellationToken);

        return serviceAreasOneOf.Match<IActionResult>(
            serviceAreas => Ok(new QueryablePaging<ServiceAreaResponse>(serviceAreas.Count, serviceAreas.Query.ProjectToType<ServiceAreaResponse>())),
            notFound => NotFound());
    }
}

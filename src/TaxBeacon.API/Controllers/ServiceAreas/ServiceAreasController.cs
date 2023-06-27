using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.JobTitles.Responses;
using TaxBeacon.API.Controllers.ServiceAreas.Requests;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.Administration.ServiceAreas;
using TaxBeacon.Administration.ServiceAreas.Models;

namespace TaxBeacon.API.Controllers.ServiceAreas;

[Authorize]
public class ServiceAreasController: BaseController
{
    private readonly IServiceAreaService _serviceAreaService;

    public ServiceAreasController(IServiceAreaService serviceAreaService) => _serviceAreaService = serviceAreaService;

    /// <summary>
    /// Queryable list of service areas
    /// </summary>
    /// <response code="200">Returns all service areas</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of service areas</returns>
    [HasPermissions(
        Common.Permissions.Departments.Read,
        Common.Permissions.Departments.ReadWrite,
        Common.Permissions.Departments.ReadExport,
        Common.Permissions.ServiceAreas.Read,
        Common.Permissions.ServiceAreas.ReadWrite,
        Common.Permissions.ServiceAreas.ReadExport)]
    [EnableQuery]
    [HttpGet("api/odata/serviceareas")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<ServiceAreaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<ServiceAreaResponse> Get()
    {
        var query = _serviceAreaService.QueryServiceAreas();

        return query.ProjectToType<ServiceAreaResponse>();
    }

    /// <summary>
    /// List of service areas
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /serviceareas?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns all service areas</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
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
    public async Task<IActionResult> GetServiceAreaList([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<ServiceAreaDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var serviceAreas = await _serviceAreaService.GetServiceAreasAsync(query, cancellationToken);

        return Ok(new QueryablePaging<ServiceAreaResponse>(serviceAreas.Count,
            serviceAreas.Query.ProjectToType<ServiceAreaResponse>()));
    }

    /// <summary>
    /// Endpoint to export tenant's service areas
    /// </summary>
    /// <param name="exportServiceAreasRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.ServiceAreas.ReadExport)]
    [HttpGet("export", Name = "ExportServiceAreas")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportServiceAreasAsync(
        [FromQuery] ExportServiceAreasRequest exportServiceAreasRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportServiceAreasRequest.FileType.ToMimeType();

        var serviceAreas = await _serviceAreaService.ExportServiceAreasAsync(
            exportServiceAreasRequest.FileType, cancellationToken);

        return File(serviceAreas, mimeType,
            $"serviceareas.{exportServiceAreasRequest.FileType.ToString().ToLowerInvariant()}");
    }

    /// <summary>
    /// Get service area details by ID
    /// </summary>
    /// <response code="200">Returns service area details with specified ID</response>
    /// <response code="404">Service area is not found</response>
    /// <returns>Service area details with specified ID</returns>
    [HasPermissions(Common.Permissions.ServiceAreas.Read, Common.Permissions.ServiceAreas.ReadWrite)]
    [HttpGet("{id:guid}", Name = "GetServiceArea")]
    [ProducesResponseType(typeof(ServiceAreaDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetServiceAreaAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var resultOneOf = await _serviceAreaService.GetServiceAreaDetailsByIdAsync(id, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            details => Ok(details.Adapt<ServiceAreaDetailsResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Get activity history log by service area ID
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="404">Service area is not found</response>
    /// <returns>List of activity logs</returns>
    [HasPermissions(Common.Permissions.ServiceAreas.Read, Common.Permissions.ServiceAreas.ReadWrite)]
    [HttpGet("{id:guid}/activities", Name = "GetServiceAreaActivityHistoryLog")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(ServiceAreaActivityHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivityHistoryAsync([FromRoute] Guid id, [FromQuery] ServiceAreaActivityHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var resultOneOf = await _serviceAreaService.GetActivityHistoryAsync(id, request.Page, request.PageSize, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<ServiceAreaActivityHistoryResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Update service area details
    /// </summary>
    /// <response code="200">Returns updated service area details</response>
    /// <response code="404">Service area is not found</response>
    /// <returns>Updated service area details</returns>
    [HasPermissions(Common.Permissions.ServiceAreas.ReadWrite)]
    [HttpPatch("{id:guid}", Name = "UpdateServiceArea")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(ServiceAreaDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateServiceAreaAsync([FromRoute] Guid id, [FromBody] UpdateServiceAreaRequest request,
        CancellationToken cancellationToken)
    {
        var resultOneOf = await _serviceAreaService.UpdateServiceAreaDetailsAsync(
            id, request.Adapt<UpdateServiceAreaDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<ServiceAreaDetailsResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Get Users of service area
    /// </summary>
    /// <response code="200">Returns Users of service area</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Service area is not found</response>
    /// <returns>Users of service area</returns>
    [HasPermissions(Common.Permissions.ServiceAreas.ReadWrite)]
    [HttpGet("{id:guid}/users", Name = "GetUsersOfServiceArea")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(ServiceAreaUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> GetUsers([FromRoute] Guid id, [FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<ServiceAreaUserDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var resultOneOf = await _serviceAreaService.GetUsersAsync(
            id, query, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            users => Ok(new QueryablePaging<ServiceAreaUserResponse>(users.Count,
                users.Query.ProjectToType<ServiceAreaUserResponse>())),
            notFound => NotFound());
    }
}

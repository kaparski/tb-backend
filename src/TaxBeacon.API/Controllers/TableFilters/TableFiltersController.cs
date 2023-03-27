using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.TableFilters.Requests;
using TaxBeacon.API.Controllers.TableFilters.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.TableFilters;

[Authorize]
[Route("api/users/{userId:guid}/[controller]")]
public class TableFiltersController: BaseController
{
    private readonly ILogger<TableFiltersController> _logger;
    private readonly ITableFiltersService _tableFiltersService;

    public TableFiltersController(ILogger<TableFiltersController> logger, ITableFiltersService tableFiltersService)
    {
        _logger = logger;
        _tableFiltersService = tableFiltersService;
    }

    /// <summary>
    /// Endpoint to get all filters for a specific table for a specific user
    /// </summary>
    /// <param name="userId">User id</param>
    /// <param name="tableType">Table type</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Filter collection successfully returned</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Collection of filters</returns>
    [HttpGet(Name = "GetFilters")]
    [HasPermissions(Common.Permissions.Filters.Read)]
    [ProducesErrorResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(List<TableFilterResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFiltersAsync([FromRoute] Guid userId, [FromQuery] EntityType tableType,
        CancellationToken cancellationToken)
    {
        var filters = await _tableFiltersService.GetFiltersAsync(
            Guid.Parse(HttpContext.User.FindFirst(Claims.TenantId)!.Value),
            userId,
            tableType,
            cancellationToken);

        return Ok(filters.Adapt<List<TableFilterResponse>>());
    }

    /// <summary>
    /// Endpoint to create a new filter for table
    /// </summary>
    /// <param name="userId">User id</param>
    /// <param name="createFilterRequest">Request with new filter data</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Filter successfully created</response>
    /// <response code="400">Invalid CreatedFilterRequest or filter name already exists</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Created filter</returns>
    [HttpPost(Name = "CreateFilter")]
    [HasPermissions(Common.Permissions.Filters.ReadWrite)]
    [ProducesErrorResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(TableFilterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(BadRequestResult), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFilterAsync([FromRoute] Guid userId,
        [FromBody] CreateFilterRequest createFilterRequest,
        CancellationToken cancellationToken)
    {
        var filterOneOf = await _tableFiltersService.AddFilterAsync(
            Guid.Parse(HttpContext.User.FindFirst(Claims.TenantId)!.Value),
            userId,
            createFilterRequest.Adapt<CreateTableFilterDto>(), cancellationToken);

        return filterOneOf.Match<IActionResult>(Ok, _ => BadRequest());
    }

    /// <summary>
    /// Endpoint to delete filter by id
    /// </summary>
    /// <param name="filterId">Filter id</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Filter successfully deleted</response>
    /// <response code="404">Filter was not found by id</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Success status code</returns>
    [HttpDelete("{filterId:guid}", Name = "DeleteFilter")]
    [HasPermissions(Common.Permissions.Filters.ReadWrite)]
    [ProducesErrorResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(OkResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(NotFoundResult), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFilterAsync([FromRoute] Guid filterId,
        CancellationToken cancellationToken)
    {
        var deleteFilterOneOf = await _tableFiltersService.DeleteFilterAsync(filterId, cancellationToken);

        return deleteFilterOneOf.Match<IActionResult>(_ => Ok(), _ => NotFound());
    }
}

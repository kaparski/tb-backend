using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.TableFilters.Requests;
using TaxBeacon.API.Controllers.TableFilters.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.TableFilters;
using TaxBeacon.UserManagement.TableFilters.Models;

namespace TaxBeacon.API.Controllers.TableFilters;

[Authorize]
public class TableFiltersController: BaseController
{
    private readonly ITableFiltersService _tableFiltersService;

    public TableFiltersController(ITableFiltersService tableFiltersService) =>
        _tableFiltersService = tableFiltersService;

    /// <summary>
    /// Endpoint to get all filters for a specific table for a specific user
    /// </summary>
    /// <param name="tableType">Table type</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Filter collection successfully returned</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Collection of filters</returns>
    [HttpGet(Name = "GetFilters")]
    [HasPermissions(Common.Permissions.TableFilters.Read)]
    [ProducesErrorResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(List<TableFilterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetFiltersAsync([FromQuery] EntityType tableType,
        CancellationToken cancellationToken)
    {
        var filters = await _tableFiltersService.GetFiltersAsync(tableType, cancellationToken);

        return Ok(filters.Adapt<List<TableFilterResponse>>());
    }

    /// <summary>
    /// Endpoint to create a new filter for table
    /// </summary>
    /// <param name="createTableFilterRequest">Request with new filter data</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Filter successfully created</response>
    /// <response code="400">Invalid CreatedFilterRequest</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="409">A filter with this name already exists</response>
    /// <returns>Created filter</returns>
    [HttpPost(Name = "CreateFilter")]
    [HasPermissions(Common.Permissions.TableFilters.ReadWrite)]
    [ProducesErrorResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(List<TableFilterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateFilterAsync([FromBody] CreateTableFilterRequest createTableFilterRequest,
        CancellationToken cancellationToken)
    {
        var filterOneOf =
            await _tableFiltersService.CreateFilterAsync(createTableFilterRequest.Adapt<CreateTableFilterDto>(),
                cancellationToken);

        return filterOneOf.Match<IActionResult>(
            filters => Ok(filters.Adapt<List<TableFilterResponse>>()),
            nameAlreadyExists => Conflict());
    }

    /// <summary>
    /// Endpoint to delete filter by id
    /// </summary>
    /// <param name="filterId">Filter id</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Filter successfully deleted</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Filter was not found by id</response>
    /// <returns>Success status code</returns>
    [HttpDelete("{filterId:guid}", Name = "DeleteFilter")]
    [HasPermissions(Common.Permissions.TableFilters.ReadWrite)]
    [ProducesErrorResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(List<TableFilterResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFilterAsync([FromRoute] Guid filterId,
        CancellationToken cancellationToken)
    {
        var deleteFilterOneOf = await _tableFiltersService.DeleteFilterAsync(filterId, cancellationToken);

        return deleteFilterOneOf.Match<IActionResult>(
            filters => Ok(filters.Adapt<List<TableFilterResponse>>()),
            notFound => NotFound());
    }
}

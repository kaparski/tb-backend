using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Locations;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Locations.Responses;
using TaxBeacon.API.Exceptions;

namespace TaxBeacon.API.Controllers.Locations;

[Authorize]
public class LocationsController: BaseController
{
    private readonly ILocationService _locationService;

    public LocationsController(ILocationService locationService) => _locationService = locationService;

    /// <summary>
    /// Get locations of an account
    /// </summary>
    /// <response code="200">Returns account's locations</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Account is not found</response>
    /// <returns>A collection of locations associated with a particular account</returns>
    [HasPermissions(Common.Permissions.Locations.Read)]
    [EnableQuery]
    [HttpGet("/api/accounts/{id:guid}/locations")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get([FromRoute] Guid accountId)
    {
        var result = _locationService.QueryLocations(accountId);

        return result.Match<IActionResult>(
            query => Ok(query.ProjectToType<LocationResponse>()),
            _ => NotFound());
    }
}

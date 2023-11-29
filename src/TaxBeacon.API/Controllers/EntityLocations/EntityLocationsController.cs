using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.Accounts.EntityLocations;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.EntityLocations.Requests;
using TaxBeacon.API.Exceptions;

namespace TaxBeacon.API.Controllers.EntityLocations;

[Authorize]
/// <response code="401">User is unauthorized</response>
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
/// <response code="403">The user does not have the required permission</response>
[ProducesResponseType(StatusCodes.Status403Forbidden)]
public class EntityLocationsController: BaseController
{
    private readonly IEntityLocationService _entityLocationsService;

    public EntityLocationsController(IEntityLocationService entityLocationsService) => _entityLocationsService = entityLocationsService;

    /// <summary>
    /// Unassociate location with entity
    /// </summary>
    /// <response code="204">Location has been successfully unassociated</response>
    /// <response code="404">Entity or location with provided ids don't exist</response>
    /// <returns>Location has been successfully unassociated</returns>
    [HasPermissions(Common.Permissions.Entities.ReadWrite, Common.Permissions.Locations.ReadWrite)]
    [HttpDelete("/api/entities/{entityId:guid}/locations/{locationId:guid}", Name = "UnassociateLocationWithEntity")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassociateLocationWithEntityAsync([FromRoute] Guid entityId,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken = default)
    {
        var result = await _entityLocationsService.UnassociateLocationWithEntityAsync(entityId,
            locationId,
            cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            _ => NotFound());
    }

    /// <summary>
    /// Link entities to location
    /// </summary>
    /// <response code="204">Entities have been successfully linked to location</response>
    /// <response code="404">Location or Account with provided id don't exist</response>
    /// <returns>Link was successfully created</returns>
    [HasPermissions(Common.Permissions.Locations.ReadWrite)]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPost("/api/accounts/{accountId:guid}/locations/{locationId:guid}/entities", Name = "AssociateEntitiesToLocation")]
    public async Task<IActionResult> AssociateEntitiesToLocationAsync([FromRoute] Guid accountId, [FromRoute] Guid locationId, [FromBody] AssociateEntitiesToLocationRequest linkRequest, CancellationToken cancellationToken)
    {
        var result = await _entityLocationsService.AssociateEntitiesToLocation(accountId, locationId, linkRequest.EntityIds, cancellationToken);
        return result.Match<IActionResult>(
            _ => NoContent(),
            _ => NotFound());
    }

    /// <summary>
    /// Link locations to entity
    /// </summary>
    /// <response code="204">Locations have been successfully linked to entity</response>
    /// <response code="404">Entity or Account with provided id don't exist</response>
    /// <returns>Link was successfully created</returns>
    [HasPermissions(Common.Permissions.Entities.ReadWrite)]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [HttpPost("/api/accounts/{accountId:guid}/entities/{entityId:guid}/locations", Name = "AssociateLocationsToEntity")]
    public async Task<IActionResult> AssociateLocationsToEntityAsync([FromRoute] Guid accountId, [FromRoute] Guid entityId, [FromBody] AssociateLocationsToEntityRequest linkRequest, CancellationToken cancellationToken)
    {
        var result = await _entityLocationsService.AssociateLocationsToEntity(accountId, entityId, linkRequest.LocationIds, cancellationToken);
        return result.Match<IActionResult>(
            _ => NoContent(),
            _ => NotFound());
    }
}

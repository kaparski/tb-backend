using FluentValidation.Results;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Locations;
using TaxBeacon.Accounts.Locations.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Locations.Requests;
using TaxBeacon.API.Controllers.Locations.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Locations;

[Authorize]
/// <response code="401">User is unauthorized</response>
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
/// <response code="403">The user does not have the required permission</response>
[ProducesResponseType(StatusCodes.Status403Forbidden)]
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
    [HasPermissions(
        Common.Permissions.Locations.Read,
        Common.Permissions.Locations.ReadActivation,
        Common.Permissions.Locations.ReadWrite,
        Common.Permissions.Locations.ReadExport)]
    [EnableQuery]
    [HttpGet("api/accounts/{accountId:guid}/locations")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<LocationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get([FromRoute] Guid accountId)
    {
        var result = _locationService.QueryLocations(accountId);

        return result.Match<IActionResult>(
            query => Ok(query.ProjectToType<LocationResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Get Location By Id
    /// </summary>
    /// <response code="200">Returns Location Details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Location is not found</response>
    /// <returns>Location details</returns>
    [HasPermissions(
        Common.Permissions.Locations.Read,
        Common.Permissions.Locations.ReadActivation,
        Common.Permissions.Locations.ReadWrite)]
    [HttpGet("~/api/accounts/{accountId:guid}/locations/{locationId:guid}", Name = "LocationDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(LocationDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLocationDetailsAsync([FromRoute] Guid accountId,
        [FromRoute] Guid locationId,
        CancellationToken cancellationToken)
    {
        var oneOfLocationDetails =
            await _locationService.GetLocationDetailsAsync(accountId, locationId, cancellationToken);

        return oneOfLocationDetails.Match<IActionResult>(
            result => Ok(result.Adapt<LocationDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Create a new location
    /// </summary>
    /// <response code="200">Returns Location Details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">No account or organization with such an ID was found</response>
    /// <returns>Location details</returns>
    [HasPermissions(Common.Permissions.Locations.ReadWrite)]
    [HttpPost("~/api/accounts/{accountId:guid}/locations", Name = "CreateLocation")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(LocationDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateNewLocationAsync([FromRoute] Guid accountId,
        [FromBody] CreateLocationRequest createLocationRequest,
        CancellationToken cancellationToken)
    {
        var createLocationResult = await _locationService.CreateNewLocationAsync(accountId,
            createLocationRequest.Adapt<CreateLocationDto>(),
            cancellationToken);

        return createLocationResult.Match<IActionResult>(
            result => Ok(result.Adapt<LocationDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Update Location status
    /// </summary>
    /// <response code="200">Returns updated location</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Location is not found</response>
    /// <returns>Updated Location details</returns>
    [HasPermissions(Common.Permissions.Locations.ReadActivation)]
    [HttpPatch("~/api/accounts/{accountId:guid}/locations/{locationId:guid}/status", Name = "UpdateLocationStatus")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(LocationDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLocationStatusAsync([FromRoute] Guid accountId,
        [FromRoute] Guid locationId,
        [FromBody] Status status, CancellationToken cancellationToken)
    {
        var resultOneOf = await _locationService.UpdateLocationStatusAsync(accountId, locationId, status, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<LocationDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Edit location
    /// </summary>
    /// <response code="200">Returns updated location</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Location is not found</response>
    /// <returns>Updated location details</returns>
    [HasPermissions(Common.Permissions.Locations.ReadWrite)]
    [HttpPatch("~/api/accounts/{accountId:guid}/locations/{locationId:guid}", Name = "UpdateLocation")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(LocationDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateLocationAsync([FromRoute] Guid accountId,
        [FromRoute] Guid locationId,
        [FromBody] UpdateLocationRequest request,
        CancellationToken cancellationToken)
    {
        var resultOneOf =
            await _locationService.UpdateLocationAsync(accountId, locationId, request.Adapt<UpdateLocationDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<LocationDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Get Locations Activity History
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Location is not found</response>
    /// <returns>Activity history for a specific location</returns>
    [HasPermissions(Common.Permissions.Locations.Read, Common.Permissions.Locations.ReadWrite)]
    [HttpGet("~/api/accounts/{accountId:guid}/locations/{locationId:guid}/activities", Name = "LocationActivityHistory")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<LocationActivityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LocationActivitiesHistory([FromRoute] Guid accountId,
        [FromRoute] Guid locationId,
        [FromQuery] LocationActivityRequest request,
        CancellationToken cancellationToken)
    {
        var activities = await _locationService.GetActivitiesAsync(accountId, locationId,
            request.Page, request.PageSize, cancellationToken);

        return activities.Match<IActionResult>(
            result => Ok(result.Adapt<LocationActivityResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Export list of account's locations
    /// </summary>
    /// <response code="200">Returns a file</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Account with specified Id is not found</response>
    /// <returns>File</returns>
    [HasPermissions(
        Common.Permissions.Locations.ReadExport)]
    [HttpGet("/api/accounts/{accountId:guid}/locations/export", Name = "ExportLocations")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportLocationsAsync([FromRoute] Guid accountId,
        [FromQuery] ExportLocationsRequest exportRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportRequest.FileType.ToMimeType();

        var result = await _locationService.ExportLocationsAsync(accountId, exportRequest.FileType, cancellationToken);

        return result.Match<IActionResult>(
            bytes => File(bytes, mimeType, $"locations.{exportRequest.FileType.ToString().ToLowerInvariant()}"),
            _ => NotFound());
    }

    /// <summary>
    /// Generate unique LocationId for tenantId
    /// </summary>
    /// <response code="200">Unique Location Id</response>
    /// <response code="401">User is unauthorized</response>
    /// <returns>Location details</returns>
    [HasPermissions(Common.Permissions.Locations.ReadWrite)]
    [HttpGet("/api/locations/generate-unique-location-id", Name = "GenerateLocationId")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateLocationIdAsync(CancellationToken cancellationToken)
    {
        var result =
            await _locationService.GenerateUniqueLocationIdAsync(cancellationToken);

        return result.Match<IActionResult>(
            code => Ok(code),
            error => BadRequest(error.Message));
    }
}

using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Entities;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Entities.Requests;
using TaxBeacon.API.Controllers.Entities.Responses;
using TaxBeacon.API.Exceptions;

namespace TaxBeacon.API.Controllers.Entities;

[Authorize]
public class EntitiesController: BaseController
{
    private readonly IEntityService _entityService;
    public EntitiesController(IEntityService entityService) => _entityService = entityService;

    /// <summary>
    /// Queryable list of entities
    /// </summary>
    /// <response code="200">Returns entities</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of entities</returns>
    [HasPermissions(
        Common.Permissions.Entities.Read)]
    [EnableQuery]
    [HttpGet("api/accounts/{accountId:guid}/entities")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<EntityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Get([FromRoute] Guid accountId)
    {
        var oneOf = _entityService.QueryEntitiesAsync(accountId);

        return oneOf.Match<IActionResult>(
            entities => Ok(entities.ProjectToType<EntityResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Get Entities Activity History
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Entity is not found</response>
    /// <returns>Activity history for a specific entity</returns>
    [HasPermissions(Common.Permissions.Entities.Read, Common.Permissions.Entities.ReadWrite)]
    [HttpGet("{id:guid}/activities", Name = "EntityActivityHistory")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<EntityActivityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EntitiesActivitiesHistory([FromRoute] Guid id, [FromQuery] EntityActivityRequest request,
        CancellationToken cancellationToken)
    {
        var activities = await _entityService.GetActivitiesAsync(id, request.Page, request.PageSize, cancellationToken);

        return activities.Match<IActionResult>(
            result => Ok(result.Adapt<EntityActivityResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Get Entity By Id
    /// </summary>
    /// <response code="200">Returns Entity Details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Entity is not found</response>
    /// <returns>Entity details</returns>
    [HasPermissions(Common.Permissions.Entities.Read, Common.Permissions.Entities.ReadWrite)]
    [HttpGet("{id:guid}", Name = "EntityDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<EntityDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEntityDetails([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var oneOfEntityDetails = await _entityService.GetEntityDetailsAsync(id, cancellationToken);

        return oneOfEntityDetails.Match<IActionResult>(
            result => Ok(result.Adapt<EntityDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Update Entity details
    /// </summary>
    /// <response code="200">Returns updated entity</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Entity is not found</response>
    /// <returns>Updated entity</returns>
    [HasPermissions(Common.Permissions.Entities.ReadWrite)]
    [HttpPatch("{id:guid}", Name = "UpdateEntity")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(EntityResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEntityAsync([FromRoute] Guid id, [FromBody] UpdateEntityRequest request,
        CancellationToken cancellationToken)
    {
        var resultOneOf = await _entityService.UpdateEntityAsync(id, request.Adapt<UpdateEntityDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<EntityResponse>()),
            notFound => NotFound());
    }
}

using FluentValidation.Results;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Entities;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.StateIds.Requests;
using TaxBeacon.API.Controllers.StateIds.Responses;
using TaxBeacon.API.Exceptions;

namespace TaxBeacon.API.Controllers.StateIds;

[Authorize]
public class StateIdsController: BaseController
{
    private readonly IEntityService _entityService;

    public StateIdsController(IEntityService entityService) => _entityService = entityService;

    /// <summary>
    /// Get Entity's StateIDs
    /// </summary>
    /// <response code="200">Returns Entity's StateIds</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Entity details</returns>
    [HasPermissions(
        Common.Permissions.Entities.Read)]
    [EnableQuery]
    [HttpGet("api/odata/entities/{entityId:guid}/stateIds", Name = "StateIds")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<StateIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetStateIds([FromRoute] Guid entityId) =>
        Ok(_entityService.GetEntityStateIdsAsync(entityId).ProjectToType<StateIdResponse>());

    /// <summary>
    /// Remove state id from entity
    /// </summary>
    /// <response code="204">State id has been successfully removed</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">An entity with provided ids doesn't exist</response>
    /// <returns>State id has been successfully removed</returns>
    [HasPermissions(Common.Permissions.Entities.ReadWrite)]
    [HttpDelete("~/api/entities/{entityId:guid}/stateids/{stateId:guid}", Name = "RemoveStateIdFromEntity")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveStateIdAsync([FromRoute] Guid entityId,
        [FromRoute] Guid stateId,
        CancellationToken cancellationToken = default)
    {
        var result = await _entityService.RemoveStateIdFromEntityAsync(entityId,
            stateId,
            cancellationToken);

        return result.Match<IActionResult>(
            _ => NoContent(),
            _ => NotFound());
    }

    /// <summary>
    /// Create new State Ids
    /// </summary>
    /// <response code="200">Returns Entity's StateIds</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Entity with provided id doesn't exist</response>
    /// <returns>State Id details</returns>
    [HasPermissions(Common.Permissions.Entities.ReadWrite)]
    [HttpPost("~/api/entities/{entityId:guid}/stateIds", Name = "AddStateIds")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(List<StateIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddStateIdsAsync([FromRoute] Guid entityId,
        [FromBody] AddStateIdsRequest addStateIdsRequest,
        CancellationToken cancellationToken = default)
    {
        var createResult = await _entityService.AddStateIdsAsync(entityId,
            addStateIdsRequest.StateIdsToAdd.Adapt<List<AddStateIdDto>>(),
            cancellationToken);

        return createResult.Match<IActionResult>(
            stateIds => Ok(stateIds.Adapt<List<StateIdResponse>>()),
            _ => NotFound());
    }

    /// <summary>
    /// Update State Id by id
    /// </summary>
    /// <response code="200">Returns Entity's StateIds</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">State Id with provided id doesn't exist</response>
    /// <returns>State Id details</returns>
    [HasPermissions(Common.Permissions.Entities.ReadWrite)]
    [HttpPatch("~/api/entities/{entityId:guid}/stateIds/{stateId:guid}", Name = "UpdateStateId")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(List<StateIdResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStateIdAsync([FromRoute] Guid entityId,
        [FromRoute] Guid stateId,
        [FromBody] UpdateStateIdRequest updateStateIdRequest,
        CancellationToken cancellationToken = default)
    {
        var updateResult = await _entityService.UpdateStateIdAsync(entityId,
            stateId,
            updateStateIdRequest.Adapt<UpdateStateIdDto>(),
            cancellationToken);

        return updateResult.Match<IActionResult>(
            stateIdDto => Ok(stateIdDto.Adapt<StateIdResponse>()),
            _ => NotFound());
    }
}

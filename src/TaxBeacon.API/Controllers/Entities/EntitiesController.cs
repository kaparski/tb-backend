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
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.FeatureManagement;

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
        Common.Permissions.Entities.Read,
        Common.Permissions.Entities.ReadActivation,
        Common.Permissions.Entities.ReadWrite,
        Common.Permissions.Entities.ReadExport)]
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
    public async Task<IActionResult> EntitiesActivitiesHistory([FromRoute] Guid id,
        [FromQuery] EntityActivityRequest request,
        CancellationToken cancellationToken)
    {
        var activities = await _entityService.GetActivitiesAsync(id, request.Page, request.PageSize, cancellationToken);

        return activities.Match<IActionResult>(
            result => Ok(result.Adapt<EntityActivityResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Get Entity By Id
    /// </summary>
    /// <response code="200">Returns Entity Details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Entity is not found</response>
    /// <returns>Entity details</returns>
    [HasPermissions(
        Common.Permissions.Entities.Read,
        Common.Permissions.Entities.ReadWrite,
        Common.Permissions.Entities.ReadActivation)]
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
    /// <response code="409">The entity with such name, or FEIN, or EIN already exists</response>
    /// <returns>Updated entity</returns>
    [HasPermissions(Common.Permissions.Entities.ReadWrite)]
    [HttpPatch("{id:guid}", Name = "UpdateEntity")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(EntityDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateEntityAsync([FromRoute] Guid id,
        [FromBody] UpdateEntityRequest request,
        CancellationToken cancellationToken)
    {
        var resultOneOf =
            await _entityService.UpdateEntityAsync(id, request.Adapt<UpdateEntityDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<EntityDetailsResponse>()),
            _ => NotFound(),
            error => Conflict(error.Adapt<InvalidOperationResponse>()));
    }

    /// <summary>
    /// Update Entity status
    /// </summary>
    /// <response code="200">Returns updated entity</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Entity is not found</response>
    /// <returns>Updated entity details</returns>
    [HasPermissions(Common.Permissions.Entities.ReadActivation)]
    [HttpPatch("{id:guid}/status", Name = "UpdateEntityStatus")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(EntityDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEntityStatusAsync([FromRoute] Guid id,
        [FromBody] Status status, CancellationToken cancellationToken)
    {
        var resultOneOf = await _entityService.UpdateEntityStatusAsync(id, status, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<EntityDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Export list of account's entities
    /// </summary>
    /// <response code="200">Returns a file</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Account with specified Id is not found</response>
    /// <returns>File</returns>
    [HasPermissions(
        Common.Permissions.Entities.ReadExport)]
    [HttpGet("/api/accounts/{accountId:guid}/entities/export", Name = "ExportAccountEntities")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportAccountEntitiesAsync([FromRoute] Guid accountId,
        [FromQuery] ExportAccountEntitiesRequest exportRequest, CancellationToken cancellationToken)
    {
        var mimeType = exportRequest.FileType.ToMimeType();

        var result = await _entityService.ExportAccountEntitiesAsync(accountId,
            exportRequest.FileType, cancellationToken);

        return result.Match<IActionResult>(
            file => File(file.FileStream, file.ContentType, file.FileName),
            _ => NotFound());
    }

    /// <summary>
    /// Create a new entity
    /// </summary>
    /// <response code="200">Returns created entity</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">An account with that id doesn't exist</response>
    /// <response code="409">The entity with such name, or FEIN, or EIN already exists</response>
    /// <returns>Created entity details</returns>
    [HasPermissions(Common.Permissions.Entities.ReadWrite)]
    [HttpPost("~/api/accounts/{accountId:guid}/entities", Name = "CreateNewEntity")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(EntityDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateNewEntityAsync([FromRoute] Guid accountId,
        [FromBody] CreateEntityRequest createEntityRequest,
        CancellationToken cancellationToken = default)
    {
        var createResult = await _entityService.CreateNewEntityAsync(accountId,
            createEntityRequest.Adapt<CreateEntityDto>(),
            cancellationToken);

        return createResult.Match<IActionResult>(
            newEntity => Ok(newEntity.Adapt<EntityDetailsResponse>()),
            _ => new NotFoundResult(),
            error => Conflict(error.Adapt<InvalidOperationResponse>()));
    }

    /// <summary>
    /// Generate unique EntityId for tenantId
    /// </summary>
    /// <response code="200">Unique Entity Id</response>
    /// <response code="401">User is unauthorized</response>
    /// <returns>Entity details</returns>
    [HasPermissions(Common.Permissions.Entities.ReadWrite)]
    [HttpGet("/api/entities/generate-unique-entity-id", Name = "GenerateEntityId")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateEntityIdAsync(CancellationToken cancellationToken)
    {
        var result =
            await _entityService.GenerateUniqueEntityIdAsync(cancellationToken);

        return result.Match<IActionResult>(
            code => Ok(code),
            error => BadRequest(error.Message));
    }

    /// <summary>
    /// Import list of account's entities
    /// </summary>
    /// <response code="200">Returns empty response</response>
    /// <response code="400">Import failed</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Account with specified Id is not found</response>
    /// <returns>Empty response</returns>
    [HasPermissions(
        Common.Permissions.Entities.Import)]
    [HttpPost("/api/accounts/{accountId:guid}/entities/import", Name = "ImportAccountEntities")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ImportAccountEntitiesAsync([FromRoute] Guid accountId,
        [FromForm] ImportEntitiesRequest importRequest, CancellationToken cancellationToken)
    {
        var result = await _entityService.ImportAccountEntitiesAsync(accountId,
            importRequest.File.OpenReadStream(), cancellationToken);

        return result.Match<IActionResult>(
            _ => Ok(),
            error => BadRequest(error.Message));
    }
}

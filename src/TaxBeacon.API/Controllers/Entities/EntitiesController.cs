using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Entities;
using TaxBeacon.API.Authentication;
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
}

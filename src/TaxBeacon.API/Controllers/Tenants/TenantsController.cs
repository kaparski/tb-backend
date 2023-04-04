using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Tenants;

[Authorize]
public class TenantsController: BaseController
{
    private readonly IUserService _userService;

    public TenantsController(IUserService userService) => _userService = userService;

    /// <summary>
    /// List of tenants
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /tenants?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
    ///     ```GET /tenants?page=2&amp;pageSize=5&amp;orderBy=name```
    /// </remarks>
    /// <response code="200">Returns users</response>
    /// <returns>List of users</returns>
    [HasPermissions(
        Common.Permissions.Tenants.Read,
        Common.Permissions.Tenants.ReadWrite,
        Common.Permissions.Tenants.ReadExport)]
    [HttpGet(Name = "GetTenants")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<TenantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequest), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantList([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<TenantDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var tenantsOneOf = await _userService.GetTenantsAsync(query, cancellationToken);
        return tenantsOneOf.Match<IActionResult>(
            tenants => Ok(new QueryablePaging<TenantResponse>(tenants.Count, tenants.Query.ProjectToType<TenantResponse>())),
            notFound => NotFound());
    }
}

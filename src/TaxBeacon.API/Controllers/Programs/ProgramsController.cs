using Gridify;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Programs.Responses;
using TaxBeacon.API.Exceptions;

namespace TaxBeacon.API.Controllers.Programs;

public class ProgramsController: BaseController
{
    /// <summary>
    /// Get tenant programs for table
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /teams?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DPeter```<br/><br/>
    ///     ```GET /teams?page=2&amp;pageSize=5&amp;orderBy=name```
    /// </remarks>
    /// <response code="200">Returns programs</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of Tenant Programs</returns>
    [HasPermissions(
        Common.Permissions.Programs.Read,
        Common.Permissions.Programs.ReadExport,
        Common.Permissions.Programs.ReadWrite)]
    [HttpGet("tenants/programs", Name = "GetTenantPrograms")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<TenantProgramResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantPrograms([FromQuery] GridifyQuery query)
    {
        if (!query.IsValid<TenantProgramResponse>())
        {
            return BadRequest();
        }

        var programs = new List<TenantProgramResponse>();
        return Ok(new QueryablePaging<TenantProgramResponse>(programs.Count, programs.AsQueryable()));
    }
}

using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.JobTitles.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.JobTitles;

[Authorize]
public class JobTitleUsersController: BaseController
{
    private readonly IJobTitleService _service;

    public JobTitleUsersController(IJobTitleService jobTitleService) => _service = jobTitleService;

    /// <summary>
    /// Queryable list of users in a given job title
    /// </summary>
    /// <response code="200">Returns Users of job title</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Job title is not found</response>
    /// <returns>Users of job title</returns>
    [HasPermissions(Common.Permissions.JobTitles.Read, Common.Permissions.JobTitles.ReadWrite)]
    [EnableQuery]
    [HttpGet("api/odata/jobtitles/{id:guid}/jobtitleusers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<JobTitleUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        try
        {
            var query = await _service.QueryUsersAsync(id);

            return Ok(query.ProjectToType<JobTitleUserResponse>());
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}

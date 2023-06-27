using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Departments.Responses;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.UserManagement.Departments;

namespace TaxBeacon.API.Controllers.Departments;

[Authorize]
public class DepartmentUsersController: BaseController
{
    private readonly IDepartmentService _service;

    public DepartmentUsersController(IDepartmentService departmentService) => _service = departmentService;

    /// <summary>
    /// Queryable list of users in a given department
    /// </summary>
    /// <response code="200">Returns Department Users</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">User does not have the required permission</response>
    /// <response code="404">Department is not found</response>
    /// <returns>A collection of users assigned to a particular department</returns>
    [HasPermissions(Common.Permissions.Departments.Read, Common.Permissions.Departments.ReadWrite)]
    [EnableQuery]
    [HttpGet("api/odata/departments/{id:guid}/departmentusers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<DepartmentUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid id)
    {
        try
        {
            var query = await _service.QueryDepartmentUsersAsync(id);

            return Ok(query.ProjectToType<DepartmentUserResponse>());
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }
}

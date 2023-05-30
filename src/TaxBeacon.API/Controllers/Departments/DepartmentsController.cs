using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Departments.Requests;
using TaxBeacon.API.Controllers.Departments.Responses;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Departments;

[Authorize]
public class DepartmentsController: BaseController
{
    private readonly IDepartmentService _service;

    public DepartmentsController(IDepartmentService departmentService) => _service = departmentService;

    /// <summary>
    /// List of tenant's departments
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /departments?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns departments in a given tenant</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of departments</returns>
    [HasPermissions(
        Common.Permissions.Departments.Read,
        Common.Permissions.Departments.ReadWrite,
        Common.Permissions.Departments.ReadExport,
        Common.Permissions.ServiceAreas.Read,
        Common.Permissions.ServiceAreas.ReadWrite,
        Common.Permissions.ServiceAreas.ReadExport)]
    [HttpGet(Name = "GetDepartments")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<DepartmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDepartmentList([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<DepartmentDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var departments = await _service.GetDepartmentsAsync(query, cancellationToken);
        return Ok(new QueryablePaging<DepartmentResponse>(departments.Count,
            departments.Query.ProjectToType<DepartmentResponse>()));
    }

    /// <summary>
    /// Endpoint to export tenant's departments
    /// </summary>
    /// <param name="exportDepartmentsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Departments.ReadExport)]
    [HttpGet("export", Name = "ExportDepartments")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportDepartmentsAsync(
        [FromQuery] ExportDepartmentsRequest exportDepartmentsRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportDepartmentsRequest.FileType.ToMimeType();

        var departments = await _service.ExportDepartmentsAsync(exportDepartmentsRequest.FileType, cancellationToken);

        return File(departments, mimeType,
            $"departments.{exportDepartmentsRequest.FileType.ToString().ToLowerInvariant()}");
    }

    /// <summary>
    /// Get Department's Activity History
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Department is not found</response>
    /// <returns>Activity history for a specific department</returns>
    [HasPermissions(Common.Permissions.Departments.Read, Common.Permissions.Departments.ReadWrite)]
    [HttpGet("{id:guid}/activities", Name = "DepartmentActivityHistory")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<DepartmentActivityHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivitiesHistory([FromRoute] Guid id,
        [FromQuery] DepartmentActivityHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var activities = await _service.GetActivityHistoryAsync(id, request.Page, request.PageSize, cancellationToken);

        return activities.Match<IActionResult>(
            result => Ok(result.Adapt<DepartmentActivityHistoryResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Get Department By Id
    /// </summary>
    /// <response code="200">Returns Department Details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Department is not found</response>
    /// <returns>Department details</returns>
    [HasPermissions(Common.Permissions.Departments.Read, Common.Permissions.Departments.ReadWrite)]
    [HttpGet("{id:guid}", Name = "DepartmentDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentDetails([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var oneOfDepartmentDetails = await _service.GetDepartmentDetailsAsync(id, cancellationToken);

        return oneOfDepartmentDetails.Match<IActionResult>(
            result => Ok(result.Adapt<DepartmentDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Update department details
    /// </summary>
    /// <response code="200">Returns updated department</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Department is not found</response>
    /// <returns>Updated department</returns>
    [HasPermissions(Common.Permissions.Departments.ReadWrite)]
    [HttpPatch("{id:guid}", Name = "UpdateDepartment")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(DepartmentDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDepartmentAsync([FromRoute] Guid id,
        [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var resultOneOf =
            await _service.UpdateDepartmentAsync(id, request.Adapt<UpdateDepartmentDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<DepartmentDetailsResponse>()),
            notFound => NotFound(),
            error => BadRequest(error.Message));
    }

    /// <summary>
    /// Get Department Users
    /// </summary>
    /// <response code="200">Returns Department Users</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">User does not have the required permission</response>
    /// <response code="404">Department is not found</response>
    /// <returns>A collection of users assigned to a particular department</returns>
    [HasPermissions(Common.Permissions.Departments.Read, Common.Permissions.Departments.ReadWrite)]
    [HttpGet("{id:guid}/users", Name = "DepartmentUsers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<DepartmentUserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentUsers([FromQuery] GridifyQuery query, [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<DepartmentUserDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var oneOfDepartmentUsers = await _service.GetDepartmentUsersAsync(id, query, cancellationToken);

        return oneOfDepartmentUsers.Match<IActionResult>(
            result => Ok(new QueryablePaging<DepartmentUserResponse>(result.Count,
                result.Query.ProjectToType<DepartmentUserResponse>())),
            _ => NotFound());
    }

    /// <summary>
    /// Get service areas of a department
    /// </summary>
    /// <response code="200">Returns department's service areas</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Department is not found</response>
    /// <returns>A collection of service areas associated with a particular department</returns>
    [HasPermissions(Common.Permissions.Departments.Read, Common.Permissions.Departments.ReadWrite)]
    [HttpGet("{id:guid}/serviceareas", Name = "GetDepartmentServiceAreas")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<DepartmentServiceAreaResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentServiceAreasAsync([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var activities = await _service.GetDepartmentServiceAreasAsync(id, cancellationToken);

        return activities.Match<IActionResult>(
            result => Ok(result.Adapt<DepartmentServiceAreaResponse[]>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Get job titles of a department
    /// </summary>
    /// <response code="200">Returns department's job titles</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Department is not found</response>
    /// <returns>A collection of job titles associated with a particular department</returns>
    [HasPermissions(Common.Permissions.Departments.Read, Common.Permissions.Departments.ReadWrite)]
    [HttpGet("{id:guid}/jobtitles", Name = "GetDepartmentJobTitles")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<DepartmentJobTitleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDepartmentJobTitlesAsync([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var activities = await _service.GetDepartmentJobTitlesAsync(id, cancellationToken);

        return activities.Match<IActionResult>(
            result => Ok(result.Adapt<DepartmentJobTitleResponse[]>()),
            notFound => NotFound());
    }
}

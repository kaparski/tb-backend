using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OneOf.Types;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Departments.Requests;
using TaxBeacon.API.Controllers.Departments.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Departments;

[Authorize]
public class DepartmentsController: BaseController
{
    private readonly IDepartmentService _service;
    private readonly ICurrentUserService _currentUserService;

    public DepartmentsController(IDepartmentService departmentService, ICurrentUserService currentUserService)
    {
        _service = departmentService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// List of tenant's departments
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /departments?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns departments in a given tenant</response>
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

        var departmentsOneOf = await _service.GetDepartmentsAsync(_currentUserService.TenantId, query, cancellationToken);
        return departmentsOneOf.Match<IActionResult>(
            departments => Ok(new QueryablePaging<DepartmentResponse>(departments.Count, departments.Query.ProjectToType<DepartmentResponse>())),
            notFound => NotFound());
    }

    /// <summary>
    /// Endpoint to export tenant's departments
    /// </summary>
    /// <param name="exportDepartmentsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Departments.ReadExport)]
    [HttpGet("export", Name = "ExportDepartments")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportDepartmentsAsync([FromQuery] ExportDepartmentsRequest exportDepartmentsRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportDepartmentsRequest.FileType.ToMimeType();

        var departments = await _service.ExportDepartmentsAsync(_currentUserService.TenantId, exportDepartmentsRequest.FileType, cancellationToken);

        return File(departments, mimeType, $"departments.{exportDepartmentsRequest.FileType.ToString().ToLowerInvariant()}");
    }

    /// <summary>
    /// Get Department's Activity History
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="404">Department is not found</response>
    [HasPermissions(Common.Permissions.Departments.Read, Common.Permissions.Departments.ReadWrite)]
    [HttpGet("{id:guid}/activities", Name = "DepartmentActivityHistory")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<DepartmentActivityHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivitiesHistory([FromRoute] Guid id, [FromQuery] DepartmentActivityHistoryRequest request,
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
    /// <response code="404">Department is not found</response>
    [HasPermissions(Common.Permissions.Departments.Read, Common.Permissions.Departments.ReadWrite)]
    [HttpGet("{id:guid}", Name = "DepartmentDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<DepartmentDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
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
    /// <response code="404">Department is not found</response>
    /// <returns>Updated department</returns>
    [HasPermissions(Common.Permissions.Departments.ReadWrite)]
    [HttpPatch("{id:guid}", Name = "UpdateDepartment")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(DepartmentDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDepartmentAsync([FromRoute] Guid id, [FromBody] UpdateDepartmentRequest request, CancellationToken cancellationToken)
    {
        var resultOneOf = await _service.UpdateDepartmentAsync(id, request.Adapt<UpdateDepartmentDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<DepartmentDetailsResponse>()),
            notFound => NotFound());
    }
}

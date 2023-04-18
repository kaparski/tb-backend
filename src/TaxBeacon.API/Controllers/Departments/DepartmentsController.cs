using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    private readonly ITenantService _tenantService;
    private readonly ICurrentUserService _currentUserService;

    public DepartmentsController(ITenantService tenantService, ICurrentUserService currentUserService)
    {
        _tenantService = tenantService;
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
        Common.Permissions.Departments.ReadExport)]
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

        var departmentsOneOf = await _tenantService.GetDepartmentsAsync(_currentUserService.TenantId, query, cancellationToken);
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

        var departments = await _tenantService.ExportDepartmentsAsync(_currentUserService.TenantId, exportDepartmentsRequest.FileType, cancellationToken);

        return File(departments, mimeType, $"departments.{exportDepartmentsRequest.FileType.ToString().ToLowerInvariant()}");
    }
}

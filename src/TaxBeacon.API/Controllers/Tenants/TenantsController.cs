using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Controllers.Users.Requests;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Tenants;

[Authorize]
public class TenantsController: BaseController
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService) => _tenantService = tenantService;

    /// <summary>
    /// List of tenants
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /tenants?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
    ///     ```GET /tenants?page=2&amp;pageSize=5&amp;orderBy=name```
    /// </remarks>
    /// <response code="200">Returns tenants</response>
    /// <returns>List of tenants</returns>
    [HasPermissions(
        Common.Permissions.Tenants.Read,
        Common.Permissions.Tenants.ReadWrite,
        Common.Permissions.Tenants.ReadExport)]
    [HttpGet(Name = "GetTenants")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<TenantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTenantList([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<TenantDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var tenantsOneOf = await _tenantService.GetTenantsAsync(query, cancellationToken);
        return tenantsOneOf.Match<IActionResult>(
            tenants => Ok(new QueryablePaging<TenantResponse>(tenants.Count, tenants.Query.ProjectToType<TenantResponse>())),
            notFound => NotFound());
    }

    /// <summary>
    /// Endpoint to export tenants
    /// </summary>
    /// <param name="exportTenantsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Tenants.ReadExport)]
    [HttpGet("export", Name = "ExportTenants")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportTenantsAsync([FromQuery] ExportTenantsRequest exportTenantsRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportTenantsRequest.FileType.ToMimeType();

        var tenants = await _tenantService.ExportTenantsAsync(exportTenantsRequest.FileType, cancellationToken);

        return File(tenants, mimeType, $"tenants.{exportTenantsRequest.FileType.ToString().ToLowerInvariant()}");
    }

    /// <summary>
    /// List of tenant's departments
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /tenants/10000001-2002-3003-4004-500000000005/departments?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns departments in a given tenant</response>
    /// <returns>List of departments</returns>
    [HasPermissions(
        Common.Permissions.Tenants.Read,
        Common.Permissions.Tenants.ReadWrite,
        Common.Permissions.Tenants.ReadExport)]
    [HttpGet("{tenantId:guid}/departments", Name = "GetDepartments")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<DepartmentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetDepartmentList([FromRoute] Guid tenantId, [FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<DepartmentDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var departmentsOneOf = await _tenantService.GetDepartmentsAsync(tenantId, query, cancellationToken);
        return departmentsOneOf.Match<IActionResult>(
            departments => Ok(new QueryablePaging<DepartmentResponse>(departments.Count, departments.Query.ProjectToType<DepartmentResponse>())),
            notFound => NotFound());
    }

    /// <summary>
    /// Endpoint to export tenant's departments
    /// </summary>
    /// <param name="tenantId"></param>
    /// <param name="exportDepartmentsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Tenants.ReadExport)]
    [HttpGet("{tenantId:guid}/departments/export", Name = "ExportDepartments")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportDepartmentsAsync([FromRoute] Guid tenantId, [FromQuery] ExportDepartmentsRequest exportDepartmentsRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportDepartmentsRequest.FileType.ToMimeType();

        var departments = await _tenantService.ExportDepartmentsAsync(tenantId, exportDepartmentsRequest.FileType, cancellationToken);

        return File(departments, mimeType, $"departments.{exportDepartmentsRequest.FileType.ToString().ToLowerInvariant()}");
    }

    /// <summary>
    /// Get tenant by ID
    /// </summary>
    /// <response code="200">Returns tenant with specified ID</response>
    /// <response code="401">Tenant is not found</response>
    /// <returns>Tenant with specified ID</returns>
    [HasPermissions(Common.Permissions.Tenants.Read)]
    [HttpGet("{id:guid}", Name = "GetTenant")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var resultOneOf = await _tenantService.GetTenantByIdAsync(id, cancellationToken);

        return resultOneOf.Match<IActionResult>(tenant => Ok(tenant.Adapt<TenantResponse>()), notFound => NotFound());
    }
}

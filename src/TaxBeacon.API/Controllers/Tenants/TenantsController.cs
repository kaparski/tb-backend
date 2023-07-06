using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Tenants.Requests;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.Administration.Tenants;
using TaxBeacon.Administration.Tenants.Models;

namespace TaxBeacon.API.Controllers.Tenants;

[Authorize]
public class TenantsController: BaseController
{
    private readonly ITenantService _tenantService;

    public TenantsController(ITenantService tenantService) => _tenantService = tenantService;

    /// <summary>
    /// Queryable list of tenants
    /// </summary>
    /// <response code="200">Returns tenants</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of tenants</returns>
    [HasPermissions(
        Common.Permissions.Tenants.Read,
        Common.Permissions.Tenants.ReadWrite,
        Common.Permissions.Tenants.ReadExport)]
    [EnableQuery]
    [HttpGet("api/odata/tenants")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<TenantResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<TenantResponse> Get()
    {
        var query = _tenantService.QueryTenants();

        return query.ProjectToType<TenantResponse>();
    }

    /// <summary>
    /// Endpoint to export tenants
    /// </summary>
    /// <param name="exportTenantsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
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
    /// Get tenant by ID
    /// </summary>
    /// <response code="200">Returns tenant with specified ID</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Tenant is not found</response>
    /// <returns>Tenant with specified ID</returns>
    [HasPermissions(Common.Permissions.Tenants.Read, Common.Permissions.Tenants.ReadWrite)]
    [HttpGet("{id:guid}", Name = "GetTenant")]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var resultOneOf = await _tenantService.GetTenantByIdAsync(id, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            tenant => Ok(tenant.Adapt<TenantResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Get activity history log by tenant ID
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Tenant is not found</response>
    /// <returns>Activity history for a specific tenant</returns>
    [HasPermissions(Common.Permissions.Tenants.Read, Common.Permissions.Tenants.ReadWrite)]
    [HttpGet("{id:guid}/activities", Name = "GetTenantActivityHistoryLog")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(TenantActivityHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivityHistoryAsync([FromRoute] Guid id,
        [FromQuery] TenantActivityHistoryRequest request, CancellationToken cancellationToken)
    {
        var resultOneOf =
            await _tenantService.GetActivityHistoryAsync(id, request.Page, request.PageSize, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<TenantActivityHistoryResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Update tenant details
    /// </summary>
    /// <response code="200">Returns updated tenant</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Tenant is not found</response>
    /// <returns>Updated tenant</returns>
    [HasPermissions(Common.Permissions.Tenants.ReadWrite)]
    [HttpPatch("{id:guid}", Name = "UpdateTenant")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTenantAsync([FromRoute] Guid id, [FromBody] UpdateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var resultOneOf =
            await _tenantService.UpdateTenantAsync(id, request.Adapt<UpdateTenantDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<TenantResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Log the fact that superAdmin has switched to a tenant
    /// </summary>
    /// <param name="newTenantId"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Log record successfully added</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Returns success response</returns>
    [HttpPost("switch", Name = "SwitchToTenant")]
    [HasPermissions(Common.Permissions.Tenants.Switch)]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(TenantResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> SwitchToTenantAsync([FromBody] Guid? newTenantId,
        CancellationToken cancellationToken)
    {
        Guid? oldTenantId = null;
        if (Guid.TryParse(Request.Headers[Headers.SuperAdminTenantId], out var headerValue))
        {
            oldTenantId = headerValue;
        }

        if (oldTenantId == null && newTenantId == null)
        {
            return Ok();
        }

        var tenantDto = await _tenantService.SwitchToTenantAsync(oldTenantId, newTenantId, cancellationToken);
        return Ok(tenantDto?.Adapt<TenantResponse>());
    }

    /// <summary>
    /// Activate/Deactivate divisions from tenant
    /// </summary>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Tenant is not found</response>
    [HasPermissions(Common.Permissions.Divisions.Activation)]
    [HttpPatch("toggle-divisions", Name = "ToggleDivisions")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleDivisionsAsync([FromQuery] bool enable, CancellationToken cancellationToken)
    {
        var resultOneOf =
            await _tenantService.ToggleDivisionsAsync(enable, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            _ => Ok(),
            _ => NotFound());
    }

    /// <summary>
    /// Get assigned tenant programs
    /// </summary>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Tenant is not found</response>
    [HasPermissions(
        Common.Permissions.Tenants.Read,
        Common.Permissions.Tenants.ReadWrite,
        Common.Permissions.Tenants.ReadExport)]
    [HttpGet("{id:guid}/programs", Name = "GetAssignedPrograms")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(List<AssignedTenantProgramResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssignedProgramsAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var changeProgramsResult = await _tenantService.GetTenantProgramsAsync(id, cancellationToken);

        return changeProgramsResult.Match<IActionResult>(
            programs => Ok(programs.Adapt<List<AssignedTenantProgramResponse>>()),
            _ => NotFound());
    }

    /// <summary>
    /// Change tenant programs
    /// </summary>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Tenant is not found</response>
    [HasPermissions(Common.Permissions.Tenants.ReadWrite)]
    [HttpPost("{id:guid}/programs", Name = "ChangePrograms")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeProgramsAsync([FromRoute] Guid id,
        [FromBody] ChangeTenantProgramsRequest changeTenantProgramsRequest,
        CancellationToken cancellationToken)
    {
        var changeProgramsResult =
            await _tenantService.ChangeTenantProgramsAsync(id, changeTenantProgramsRequest.ProgramsIds,
                cancellationToken);

        return changeProgramsResult.Match<IActionResult>(
            _ => Ok(),
            _ => NotFound());
    }
}

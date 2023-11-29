using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Administration.Programs;
using TaxBeacon.Administration.Programs.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Programs.Requests;
using TaxBeacon.API.Controllers.Programs.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Programs;

public class TenantProgramsController: BaseController
{
    private readonly IProgramService _programService;

    public TenantProgramsController(IProgramService programService) => _programService = programService;

    /// <summary>
    /// Queryable list of all tenant programs
    /// </summary>
    /// <response code="200">Returns list of all programs</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of all programs</returns>
    [HasPermissions(
        Common.Permissions.Programs.Read,
        Common.Permissions.Programs.ReadWrite,
        Common.Permissions.Programs.ReadExport)]
    [EnableQuery]
    [HttpGet("api/odata/tenantprograms")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<TenantProgramResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<TenantProgramResponse> Get()
    {
        var query = _programService.QueryTenantPrograms();

        return query.ProjectToType<TenantProgramResponse>();
    }

    /// <summary>
    /// Endpoint to export list of tenant programs
    /// </summary>
    /// <param name="exportProgramsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Programs.ReadExport)]
    [HttpGet("/api/tenants/programs/export", Name = "TenantExportPrograms")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportTenantProgramsAsync([FromQuery] ExportProgramsRequest exportProgramsRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportProgramsRequest.FileType.ToMimeType();

        var programs =
            await _programService.ExportTenantProgramsAsync(exportProgramsRequest.FileType, cancellationToken);

        return File(programs, mimeType,
            $"programs.{exportProgramsRequest.FileType.ToString().ToLowerInvariant()}");
    }

    /// <summary>
    /// Get Tenant Program details by ID
    /// </summary>
    /// <response code="200">Returns Tenant Program details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Program is not found</response>
    /// <returns>Program details</returns>
    [HasPermissions(
        Common.Permissions.Programs.Read,
        Common.Permissions.Programs.ReadWrite,
        Common.Permissions.Programs.ReadExport)]
    [HttpGet("/api/tenants/programs/{id:guid}", Name = "TenantProgramDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<TenantProgramDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantProgramDetailsAsync([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var oneOfProgramDetails = await _programService.GetTenantProgramDetailsAsync(id, cancellationToken);

        return oneOfProgramDetails.Match<IActionResult>(
            program => Ok(program.Adapt<TenantProgramDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Endpoint to update program status
    /// </summary>
    /// <param name="id">Program id</param>
    /// <param name="programStatus">New program status</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns updated program</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Updated program</returns>
    [HasPermissions(Common.Permissions.Programs.ReadActivation)]
    [HttpPut("/api/tenants/programs/{id:guid}/status", Name = "UpdateProgramStatus")]
    [ProducesResponseType(typeof(TenantProgramDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TenantProgramDetailsResponse>> UpdateProgramStatusAsync(Guid id,
        [FromBody] Status programStatus,
        CancellationToken cancellationToken)
    {
        var user = await _programService.UpdateTenantProgramStatusAsync(id, programStatus, cancellationToken);

        return Ok(user.Adapt<TenantProgramDetailsResponse>());
    }

    /// <summary>
    /// Endpoint to get tenant program assignment on org structure unit
    /// </summary>
    /// <param name="id">Program id</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns tenant program assignment</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">The the tenant program with such Id was not found</response>
    /// <returns>Updated program</returns>
    [HasPermissions(Common.Permissions.Programs.ReadTenantOrgUnits, Common.Permissions.Programs.ReadAssignTenantOrgUnits)]
    [HttpGet("/api/tenants/programs/{id:guid}/assignment", Name = "GetTenantProgramOrgUnitsAssignment")]
    [ProducesResponseType(typeof(TenantProgramOrgUnitsAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTenantProgramOrgUnitsAssignmentAsync(Guid id,
        CancellationToken cancellationToken)
    {
        var getTenantProgramOrgUnitsAssignmentResult = await _programService
            .GetTenantProgramOrgUnitsAssignmentAsync(id, cancellationToken);

        return getTenantProgramOrgUnitsAssignmentResult.Match<IActionResult>(
            assignment => Ok(assignment.Adapt<TenantProgramOrgUnitsAssignmentResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Endpoint to assign tenant program on org structure unit
    /// </summary>
    /// <param name="id">Program id</param>
    /// <param name="assignTenantProgramRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns tenant program assignment</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">The the tenant program, department or service are with such Ids was not found </response>
    /// <returns>Updated program</returns>
    [HasPermissions(Common.Permissions.Programs.ReadAssignTenantOrgUnits)]
    [HttpPost("/api/tenants/programs/{id:guid}/assignment", Name = "GetTenantProgramOrgUnitsAssignment")]
    [ProducesResponseType(typeof(TenantProgramOrgUnitsAssignmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeTenantProgramAssignmentAsync(Guid id,
        [FromBody] AssignTenantProgramRequest assignTenantProgramRequest,
        CancellationToken cancellationToken)
    {
        var getTenantProgramOrgUnitsAssignmentResult = await _programService
            .ChangeTenantProgramAssignmentAsync(id, assignTenantProgramRequest.Adapt<AssignTenantProgramDto>(),
                cancellationToken);

        return getTenantProgramOrgUnitsAssignmentResult.Match<IActionResult>(
            assignment => Ok(assignment.Adapt<TenantProgramOrgUnitsAssignmentResponse>()),
            _ => NotFound());
    }
}

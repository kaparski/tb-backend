using Gridify;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Programs.Requests;
using TaxBeacon.API.Controllers.Programs.Responses;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models.Programs;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Programs;

public class ProgramsController: BaseController
{
    private readonly IProgramService _programService;

    public ProgramsController(IProgramService programService) => _programService = programService;

    /// <summary>
    /// List of all programs
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /programs?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns list of all programs</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of all programs</returns>
    [HasPermissions(
        Common.Permissions.Programs.Read,
        Common.Permissions.Programs.ReadWrite,
        Common.Permissions.Programs.ReadExport)]
    [HttpGet(Name = "GetAllPrograms")]
    [HasSuperAdminRole]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<ProgramResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllProgramsAsync([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<ProgramDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var programs = await _programService.GetAllProgramsAsync(query, cancellationToken);

        var response = new QueryablePaging<ProgramResponse>(programs.Count,
            programs.Query.ProjectToType<ProgramResponse>());

        return Ok(response);
    }

    /// <summary>
    /// Endpoint to export list of programs
    /// </summary>
    /// <param name="exportProgramsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Programs.ReadExport)]
    [HttpGet("export", Name = "ExportPrograms")]
    [HasSuperAdminRole]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportProgramsAsync([FromQuery] ExportProgramsRequest exportProgramsRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportProgramsRequest.FileType.ToMimeType();

        var programs = await _programService.ExportProgramsAsync(exportProgramsRequest.FileType, cancellationToken);

        return File(programs, mimeType,
            $"programs.{exportProgramsRequest.FileType.ToString().ToLowerInvariant()}");
    }

    /// <summary>
    /// Get Program details by ID
    /// </summary>
    /// <response code="200">Returns Program details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Program is not found</response>
    /// <returns>Program details</returns>
    [HasPermissions(
        Common.Permissions.Programs.Read,
        Common.Permissions.Programs.ReadWrite,
        Common.Permissions.Programs.ReadExport)]
    [HttpGet("{id:guid}", Name = "ProgramDetails")]
    [HasSuperAdminRole]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<ProgramDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgramDetailsAsync([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var oneOfProgramDetails = await _programService.GetProgramDetailsAsync(id, cancellationToken);

        return oneOfProgramDetails.Match<IActionResult>(
            program => Ok(program.Adapt<ProgramDetailsResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Get Program's activity history
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Program is not found</response>
    /// <returns>Activity history for a specific program</returns>
    [HasPermissions(
        Common.Permissions.Programs.Read,
        Common.Permissions.Programs.ReadWrite,
        Common.Permissions.Programs.ReadExport)]
    [HttpGet("{id:guid}/activities", Name = "ProgramActivityHistory")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<ProgramActivityHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProgramActivityHistoryAsync([FromRoute] Guid id,
        [FromQuery] ProgramActivityHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var oneOfActivities = await _programService.GetProgramActivityHistoryAsync(
            id, request.Page, request.PageSize, cancellationToken);

        return oneOfActivities.Match<IActionResult>(
            result => Ok(result.Adapt<ProgramActivityHistoryResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Get tenant programs for table
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /tenants/programs?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DPeter```<br/><br/>
    ///     ```GET /tenants/programs?page=2&amp;pageSize=5&amp;orderBy=name```
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
    [HttpGet("/api/tenants/programs", Name = "GetTenantPrograms")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<TenantProgramResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllTenantProgramsAsync([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<TenantProgramDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var programs = await _programService.GetAllTenantProgramsAsync(query, cancellationToken);

        var response = new QueryablePaging<TenantProgramResponse>(programs.Count,
            programs.Query.ProjectToType<TenantProgramResponse>());

        return Ok(response);
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

        var programs = await _programService.ExportTenantProgramsAsync(exportProgramsRequest.FileType, cancellationToken);

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
            notFound => NotFound());
    }

    /// <summary>
    /// Update program details
    /// </summary>
    /// <response code="200">Returns updated program details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Program is not found</response>
    /// <returns>Updated program details</returns>
    [HasPermissions(Common.Permissions.Programs.ReadWrite)]
    [HttpPatch("{id:guid}", Name = "UpdateProgram")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(ProgramDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateProgramAsync([FromRoute] Guid id, [FromBody] UpdateProgramRequest request,
        CancellationToken cancellationToken)
    {
        var resultOneOf = await _programService.UpdateProgramAsync(
            id, request.Adapt<UpdateProgramDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<ProgramDetailsResponse>()),
            notFound => NotFound());
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
    [HasPermissions(Common.Permissions.Programs.ReadWrite)]
    [HttpPut("/api/tenants/programs/{id:guid}/status", Name = "UpdateProgramStatus")]
    [ProducesResponseType(typeof(TenantProgramDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TenantProgramDetailsResponse>> UpdateProgramStatusAsync(Guid id, [FromBody] Status programStatus,
        CancellationToken cancellationToken)
    {

        var user = await _programService.UpdateTenantProgramStatusAsync(id, programStatus, cancellationToken);

        return Ok(user.Adapt<TenantProgramDetailsResponse>());
    }
}

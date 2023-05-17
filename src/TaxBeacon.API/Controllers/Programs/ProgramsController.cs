using Gridify;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Programs.Requests;
using TaxBeacon.API.Controllers.Programs.Response;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
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
        var oneOfActivities = await _programService.GetProgramActivityHistory(
            id, request.Page, request.PageSize, cancellationToken);

        return oneOfActivities.Match<IActionResult>(
            result => Ok(result.Adapt<ProgramActivityHistoryResponse>()),
            notFound => NotFound());
    }
}
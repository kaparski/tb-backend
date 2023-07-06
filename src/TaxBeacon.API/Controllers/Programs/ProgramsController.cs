using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Programs.Requests;
using TaxBeacon.API.Controllers.Programs.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.Administration.Programs;
using TaxBeacon.Administration.Programs.Models;

namespace TaxBeacon.API.Controllers.Programs;

public class ProgramsController: BaseController
{
    private readonly IProgramService _programService;

    public ProgramsController(IProgramService programService) => _programService = programService;

    /// <summary>
    /// Queryable list of all programs
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
    [HttpGet("api/odata/programs")]
    [HasSuperAdminRole]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<ProgramResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<ProgramResponse> Get()
    {
        var query = _programService.QueryPrograms();

        return query.ProjectToType<ProgramResponse>();
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
            _ => NotFound());
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
            _ => NotFound());
    }

    /// <summary>
    /// Update program details
    /// </summary>
    /// <response code="200">Returns updated program details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Program is not found</response>
    /// <response code="409">A program with such name already exists</response>
    /// <returns>Updated program details</returns>
    [HasPermissions(Common.Permissions.Programs.ReadWrite)]
    [HttpPatch("{id:guid}", Name = "UpdateProgram")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(ProgramDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> UpdateProgramAsync([FromRoute] Guid id,
        [FromBody] UpdateProgramRequest request,
        CancellationToken cancellationToken)
    {
        var resultOneOf = await _programService.UpdateProgramAsync(
            id, request.Adapt<UpdateProgramDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<ProgramDetailsResponse>()),
            _ => NotFound(),
            _ => Conflict());
    }

    /// <summary>
    /// Endpoint to create a new program
    /// </summary>
    /// <param name="createProgramRequest">New program data</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns created program</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="409">A program with such name already exists</response>
    /// <returns>Created program</returns>
    [HasPermissions(Common.Permissions.Programs.ReadWrite)]
    [HttpPost(Name = "CreateProgram")]
    [ProducesResponseType(typeof(ProgramDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateProgramAsync([FromBody] CreateProgramRequest createProgramRequest,
        CancellationToken cancellationToken)
    {
        var createProgramResult =
            await _programService.CreateProgramAsync(createProgramRequest.Adapt<CreateProgramDto>(), cancellationToken);

        return createProgramResult.Match<IActionResult>(
            newProgram => Ok(newProgram.Adapt<ProgramDetailsResponse>()),
            _ => Conflict());
    }
}

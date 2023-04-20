using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Tenants.Requests;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Tenants
{

    [Authorize]
    public class DivisionsController: BaseController
    {
        private readonly IDivisionsService _divisionsService;
        public DivisionsController(IDivisionsService divisionsService) => _divisionsService = divisionsService;
        /// <summary>
        /// List of tenant divisions
        /// </summary>
        /// <remarks>
        /// Sample requests: <br/><br/>
        ///     ```GET /divisions?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
        ///     ```GET /divisons?page=2&amp;pageSize=5&amp;orderBy=name```
        /// </remarks>
        /// <response code="200">Returns tenant divisions</response>
        /// <returns>List of tenant divisions</returns>
        [HasPermissions(
            Common.Permissions.Divisions.Read,
            Common.Permissions.Divisions.ReadWrite,
            Common.Permissions.Divisions.ReadExport)]
        [HttpGet(Name = "GetDivisions")]
        [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
        [ProducesResponseType(typeof(QueryablePaging<DivisionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> GetDivisionsList([FromQuery] GridifyQuery query,
            CancellationToken cancellationToken)
        {
            if (!query.IsValid<DivisionDto>())
            {
                // TODO: Add an object with errors that we can use to detail the answers
                return BadRequest();
            }

            var divisionsOneOf = await _divisionsService.GetDivisionsAsync(query, cancellationToken);
            return divisionsOneOf.Match<IActionResult>(
                divisions => Ok(new QueryablePaging<DivisionResponse>(divisions.Count, divisions.Query.ProjectToType<DivisionResponse>())),
                notFound => NotFound());
        }

        /// <summary>
        /// Endpoint to export tenant divisions
        /// </summary>
        /// <param name="exportDivisionsRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Returns file content</response>
        /// <response code="401">User is unauthorized</response>
        /// <returns>File content</returns>
        [HasPermissions(Common.Permissions.Divisions.ReadExport)]
        [HttpGet("export", Name = "ExportTenantDivisions")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportDivisionsAsync([FromQuery] ExportDivisionsRequest exportDivisionsRequest,
            CancellationToken cancellationToken)
        {
            var mimeType = exportDivisionsRequest.FileType.ToMimeType();

            var users = await _divisionsService.ExportDivisionsAsync(exportDivisionsRequest.FileType, cancellationToken);

            return File(users, mimeType, $"tenantDivisions.{exportDivisionsRequest.FileType.ToString().ToLowerInvariant()}");
        }

        /// <summary>
        /// Get activity history log by tenant ID
        /// </summary>
        /// <response code="200">Returns activity logs</response>
        /// <response code="404">Division is not found</response>
        [HasPermissions(Common.Permissions.Divisions.Read, Common.Permissions.Divisions.ReadWrite)]
        [HttpGet("{id:guid}/activities", Name = "DivisionActivityHistory")]
        [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
        [ProducesResponseType(typeof(IEnumerable<DivisionActivityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DivisionActivitiesHistory([FromRoute] Guid id, [FromQuery] DivisionActivityRequest request,
            CancellationToken cancellationToken)
        {
            var activities = await _divisionsService.GetActivitiesAsync(id, request.Page, request.PageSize, cancellationToken);

            return activities.Match<IActionResult>(
                result => Ok(result.Adapt<DivisionActivityResponse>()),
                notFound => NotFound());
        }

        /// <summary>
        /// Get Divisions By Id
        /// </summary>
        /// <response code="200">Returns Division Details</response>
        /// <response code="404">Division is not found</response>
        [HasPermissions(Common.Permissions.Divisions.Read, Common.Permissions.Divisions.ReadWrite)]
        [HttpGet("{id:guid}", Name = "DivisionDetails")]
        [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
        [ProducesResponseType(typeof(IEnumerable<DivisionDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDivisionDetails([FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var oneOfDivisionDetails = await _divisionsService.GetDivisionDetailsAsync(id, cancellationToken);

            return oneOfDivisionDetails.Match<IActionResult>(
                result => Ok(result.Adapt<DivisionDetailsResponse>()),
                _ => NotFound());
        }
    }
}

using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
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
        /// Queryable list of tenant divisions
        /// </summary>
        /// <response code="200">Returns tenant divisions</response>
        /// <response code="400">Invalid filtering or sorting</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">The user does not have the required permission</response>
        /// <returns>List of tenant divisions</returns>
        [HasPermissions(
            Common.Permissions.Divisions.Read,
            Common.Permissions.Divisions.ReadWrite,
            Common.Permissions.Divisions.ReadExport)]
        [EnableQuery]
        [HttpGet("api/odata/divisions")]
        [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
        [ProducesResponseType(typeof(IQueryable<DivisionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IQueryable<DivisionResponse> Get()
        {
            var query = _divisionsService.QueryDivisions();

            return query.ProjectToType<DivisionResponse>();
        }

        /// <summary>
        /// List of tenant divisions
        /// </summary>
        /// <remarks>
        /// Sample requests: <br/><br/>
        ///     ```GET /divisions?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
        ///     ```GET /divisons?page=2&amp;pageSize=5&amp;orderBy=name```
        /// </remarks>
        /// <response code="200">Returns tenant divisions</response>
        /// <response code="400">Invalid filtering or sorting</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">The user does not have the required permission</response>
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
        public async Task<IActionResult> GetDivisionsList([FromQuery] GridifyQuery query,
            CancellationToken cancellationToken)
        {
            if (!query.IsValid<DivisionDto>())
            {
                // TODO: Add an object with errors that we can use to detail the answers
                return BadRequest();
            }

            var divisions = await _divisionsService.GetDivisionsAsync(query, cancellationToken);
            return Ok(new QueryablePaging<DivisionResponse>(divisions.Count,
                divisions.Query.ProjectToType<DivisionResponse>()));
        }

        /// <summary>
        /// Endpoint to export tenant divisions
        /// </summary>
        /// <param name="exportDivisionsRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Returns file content</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">The user does not have the required permission</response>
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

            var users = await _divisionsService.ExportDivisionsAsync(exportDivisionsRequest.FileType,
                cancellationToken);

            return File(users, mimeType,
                $"tenantDivisions.{exportDivisionsRequest.FileType.ToString().ToLowerInvariant()}");
        }

        /// <summary>
        /// Get Divisions Activity History
        /// </summary>
        /// <response code="200">Returns activity logs</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">The user does not have the required permission</response>
        /// <response code="404">Division is not found</response>
        /// <returns>Activity history for a specific division</returns>
        [HasPermissions(Common.Permissions.Divisions.Read, Common.Permissions.Divisions.ReadWrite)]
        [HttpGet("{id:guid}/activities", Name = "DivisionActivityHistory")]
        [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
        [ProducesResponseType(typeof(IEnumerable<DivisionActivityResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DivisionActivitiesHistory([FromRoute] Guid id,
            [FromQuery] DivisionActivityRequest request,
            CancellationToken cancellationToken)
        {
            var activities =
                await _divisionsService.GetActivitiesAsync(id, request.Page, request.PageSize, cancellationToken);

            return activities.Match<IActionResult>(
                result => Ok(result.Adapt<DivisionActivityResponse>()),
                notFound => NotFound());
        }

        /// <summary>
        /// Get Divisions By Id
        /// </summary>
        /// <response code="200">Returns Division Details</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">The user does not have the required permission</response>
        /// <response code="404">Division is not found</response>
        /// <returns>A division details</returns>
        [HasPermissions(Common.Permissions.Divisions.Read, Common.Permissions.Divisions.ReadWrite)]
        [HttpGet("{id:guid}", Name = "DivisionDetails")]
        [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
        [ProducesResponseType(typeof(IEnumerable<DivisionDetailsResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDivisionDetails([FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var oneOfDivisionDetails = await _divisionsService.GetDivisionDetailsAsync(id, cancellationToken);

            return oneOfDivisionDetails.Match<IActionResult>(
                result => Ok(result.Adapt<DivisionDetailsResponse>()),
                _ => NotFound());
        }

        /// <summary>
        /// Get Users of Division
        /// </summary>
        /// <response code="200">Returns Division Users</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">The user does not have the required permission</response>
        /// <response code="404">Division is not found</response>
        /// <returns>A collection of users assigned to a particular division</returns>
        [HasPermissions(Common.Permissions.Divisions.Read, Common.Permissions.Divisions.ReadWrite)]
        [HttpGet("{id:guid}/users", Name = "DivisionUsers")]
        [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
        [ProducesResponseType(typeof(QueryablePaging<DivisionUserResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDivisionUsers([FromQuery] GridifyQuery query, [FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            if (!query.IsValid<DivisionUserDto>())
            {
                // TODO: Add an object with errors that we can use to detail the answers
                return BadRequest();
            }

            var oneOfDivisionUsers = await _divisionsService.GetDivisionUsersAsync(id, query, cancellationToken);

            return oneOfDivisionUsers.Match<IActionResult>(
                result => Ok(new QueryablePaging<DivisionUserResponse>(result.Count,
                    result.Query.ProjectToType<DivisionUserResponse>())),
                _ => NotFound());
        }

        /// <summary>
        /// Update division details
        /// </summary>
        /// <response code="200">Returns updated division</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">The user does not have the required permission</response>
        /// <response code="404">Division is not found</response>
        /// <returns>Updated division</returns>
        [HasPermissions(Common.Permissions.Divisions.ReadWrite)]
        [HttpPatch("{id:guid}", Name = "UpdateDivision")]
        [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
        [ProducesResponseType(typeof(DivisionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDivisionAsync([FromRoute] Guid id,
            [FromBody] UpdateDivisionRequest request, CancellationToken cancellationToken)
        {
            var resultOneOf =
                await _divisionsService.UpdateDivisionAsync(id, request.Adapt<UpdateDivisionDto>(), cancellationToken);

            return resultOneOf.Match<IActionResult>(
                result => Ok(result.Adapt<DivisionDetailsResponse>()),
                notFound => NotFound(),
                error => Conflict(error.Message));
        }

        /// <summary>
        /// Get departments of a division
        /// </summary>
        /// <response code="200">Returns division departments</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="403">The user does not have the required permission</response>
        /// <response code="404">Division is not found</response>
        /// <returns>A collection of departments associated with a particular division</returns>
        [HasPermissions(Common.Permissions.Divisions.Read, Common.Permissions.Divisions.ReadWrite)]
        [HttpGet("{id:guid}/departments", Name = "DivisionDepartments")]
        [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
        [ProducesResponseType(typeof(IEnumerable<DivisionDepartmentResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDivisionDepartmentsAsync([FromRoute] Guid id,
            CancellationToken cancellationToken)
        {
            var oneOfDivisionUsers = await _divisionsService.GetDivisionDepartmentsAsync(id, cancellationToken);

            return oneOfDivisionUsers.Match<IActionResult>(
                result => Ok(result.Adapt<DivisionDepartmentResponse[]>()),
                _ => NotFound());
        }
    }
}

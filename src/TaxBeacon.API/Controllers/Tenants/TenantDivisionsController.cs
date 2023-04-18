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

namespace TaxBeacon.API.Controllers.Tenants
{

    // [Authorize]
    public class TenantDivisionsController: BaseController
    {
        private readonly ITenantDivisionsService _tenantDivisionsService;
        public TenantDivisionsController(ITenantDivisionsService tenantDivisionsService) => _tenantDivisionsService = tenantDivisionsService;
        /// <summary>
        /// List of tenant divisions
        /// </summary>
        /// <remarks>
        /// Sample requests: <br/><br/>
        ///     ```GET /tenantDivisions?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
        ///     ```GET /tenantDivisons?page=2&amp;pageSize=5&amp;orderBy=name```
        /// </remarks>
        /// <response code="200">Returns tenant divisions</response>
        /// <returns>List of tenant divisions</returns>
        //[HasPermissions(
        //    Common.Permissions.Tenants.Read,
        //    Common.Permissions.Tenants.ReadWrite,
        //    Common.Permissions.Tenants.ReadExport)]
        [HttpGet(Name = "GetTenantDivisions")]
        [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
        [ProducesResponseType(typeof(QueryablePaging<TenantResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> GetTenantDivisionsList([FromQuery] GridifyQuery query,
            CancellationToken cancellationToken)
        {
            if (!query.IsValid<DivisionDto>())
            {
                // TODO: Add an object with errors that we can use to detail the answers
                return BadRequest();
            }

            var tenantsOneOf = await _tenantDivisionsService.GetTenantDivisionsAsync(query, cancellationToken);
            return tenantsOneOf.Match<IActionResult>(
                tenantDivisions => Ok(new QueryablePaging<DivisionResponse>(tenantDivisions.Count, tenantDivisions.Query.ProjectToType<DivisionResponse>())),
                notFound => NotFound());
        }

        /// <summary>
        /// Endpoint to export tenant divisions
        /// </summary>
        /// <param name="exportTenantDivisionsRequest"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Returns file content</response>
        /// <response code="401">User is unauthorized</response>
        /// <returns>File content</returns>
        [HasPermissions(Common.Permissions.Tenants.ReadExport)]
        [HttpGet("export", Name = "ExportTenantDivisions")]
        [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> ExportTenantsAsync([FromQuery] ExportTenantsRequest exportTenantDivisionsRequest,
            CancellationToken cancellationToken)
        {
            var mimeType = exportTenantDivisionsRequest.FileType.ToMimeType();

            var users = await _tenantDivisionsService.ExportTenantDivisionsAsync(exportTenantDivisionsRequest.FileType, cancellationToken);

            return File(users, mimeType, $"tenantDivisions.{exportTenantDivisionsRequest.FileType.ToString().ToLowerInvariant()}");
        }
    }
}

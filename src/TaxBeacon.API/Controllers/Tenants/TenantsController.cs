﻿using Gridify;
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
    [ProducesResponseType(StatusCodes.Status409Conflict)]
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

        var users = await _tenantService.ExportTenantsAsync(exportTenantsRequest.FileType, cancellationToken);

        return File(users, mimeType, $"tenants.{exportTenantsRequest.FileType.ToString().ToLowerInvariant()}");
    }
}
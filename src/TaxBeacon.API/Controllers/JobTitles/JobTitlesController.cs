﻿using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.JobTitles.Requests;
using TaxBeacon.API.Controllers.JobTitles.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.JobTitles;

[Authorize]
public class JobTitlesController: BaseController
{
    private readonly IJobTitleService _jobTitleService;

    public JobTitlesController(IJobTitleService jobTitleService) => _jobTitleService = jobTitleService;

    /// <summary>
    /// List of job titles
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /jobtitles?page=1&amp;pageSize=10&amp;orderBy=name%20desc&amp;filter=name%3DContoso```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns all job titles</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of job titles</returns>
    [HasPermissions(
        Common.Permissions.Departments.Read,
        Common.Permissions.Departments.ReadWrite,
        Common.Permissions.Departments.ReadExport,
        Common.Permissions.JobTitles.Read,
        Common.Permissions.JobTitles.ReadWrite,
        Common.Permissions.JobTitles.ReadExport)]
    [HttpGet(Name = "GetJobTitles")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(JobTitleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobTitleList([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<JobTitleDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var jobTitlesOneOf = await _jobTitleService.GetJobTitlesAsync(query, cancellationToken);

        return jobTitlesOneOf.Match<IActionResult>(
            jobTitles => Ok(new QueryablePaging<JobTitleResponse>(jobTitles.Count,
                jobTitles.Query.ProjectToType<JobTitleResponse>())),
            notFound => NotFound());
    }

    /// <summary>
    /// Endpoint to export tenant's job titles
    /// </summary>
    /// <param name="exportJobTitlesRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.JobTitles.ReadExport)]
    [HttpGet("export", Name = "ExportJobTitles")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportJobTitlesAsync(
        [FromQuery] ExportJobTitlesRequest exportJobTitlesRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportJobTitlesRequest.FileType.ToMimeType();

        var jobTitles = await _jobTitleService.ExportJobTitlesAsync(
            exportJobTitlesRequest.FileType, cancellationToken);

        return File(jobTitles, mimeType,
            $"jobtitles.{exportJobTitlesRequest.FileType.ToString().ToLowerInvariant()}");
    }

    /// <summary>
    /// Get job title details by ID
    /// </summary>
    /// <response code="200">Returns job title details with specified ID</response>
    /// <response code="404">Job title is not found</response>
    /// <returns>Job title details with specified ID</returns>
    [HasPermissions(Common.Permissions.JobTitles.Read, Common.Permissions.JobTitles.ReadWrite)]
    [HttpGet("{id:guid}", Name = "GetJobTitle")]
    [ProducesResponseType(typeof(JobTitleDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetJobTitleAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var resultOneOf = await _jobTitleService.GetJobTitleDetailsByIdAsync(id, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            details => Ok(details.Adapt<JobTitleDetailsResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Get activity history log by job title ID
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="404">Job title is not found</response>
    /// <returns>List of activity logs</returns>
    [HasPermissions(Common.Permissions.JobTitles.Read, Common.Permissions.JobTitles.ReadWrite)]
    [HttpGet("{id:guid}/activities", Name = "GetJobTitleActivityHistoryLog")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(JobTitleActivityHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetActivityHistoryAsync([FromRoute] Guid id, [FromQuery] JobTitleActivityHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var resultOneOf = await _jobTitleService.GetActivityHistoryAsync(id, request.Page, request.PageSize, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<JobTitleActivityHistoryResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Update job title details
    /// </summary>
    /// <response code="200">Returns updated job title details</response>
    /// <response code="404">Job title is not found</response>
    /// <returns>Updated job title details</returns>
    [HasPermissions(Common.Permissions.JobTitles.ReadWrite)]
    [HttpPatch("{id:guid}", Name = "UpdateJobTitle")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(JobTitleDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateJobTitleAsync([FromRoute] Guid id, [FromBody] UpdateJobTitleRequest request,
        CancellationToken cancellationToken)
    {
        var resultOneOf = await _jobTitleService.UpdateJobTitleDetailsAsync(
            id, request.Adapt<UpdateJobTitleDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<JobTitleDetailsResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Get Users of job title
    /// </summary>
    /// <response code="200">Returns Users of job title</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Job title is not found</response>
    /// <returns>Users of job title</returns>
    [HasPermissions(Common.Permissions.JobTitles.ReadWrite)]
    [HttpGet("{id:guid}/users", Name = "GetUsersOfJobTitle")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(JobTitleUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]

    public async Task<IActionResult> GetUsers([FromRoute] Guid id, [FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<JobTitleUserDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var resultOneOf = await _jobTitleService.GetUsersAsync(
            id, query, cancellationToken);

        return resultOneOf.Match<IActionResult>(
            users => Ok(new QueryablePaging<JobTitleUserResponse>(users.Count,
                users.Query.ProjectToType<JobTitleUserResponse>())),
            notFound => NotFound());
    }
}

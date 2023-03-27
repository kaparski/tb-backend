﻿using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Users.Requests;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Users;

[Authorize]
public class UsersController: BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>
    /// List of users
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /users?page=1&amp;pageSize=10&amp;orderBy=email%20desc&amp;filter=firstName%3DPeter```<br/><br/>
    ///     ```GET /users?page=2&amp;pageSize=5&amp;orderBy=email```
    /// </remarks>
    /// <response code="200">Returns users</response>
    /// <returns>List of users</returns>
    [HasPermissions(
        Common.Permissions.Users.Read,
        Common.Permissions.Users.ReadWrite,
        Common.Permissions.Users.ReadExport)]
    [HttpGet(Name = "GetUsers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequest), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserList([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<UserDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var usersOneOf = await _userService.GetUsersAsync(query, cancellationToken);
        return usersOneOf.Match<IActionResult>(
            users => Ok(new QueryablePaging<UserResponse>(users.Count, users.Query.ProjectToType<UserResponse>())),
            notFound => NotFound());
    }

    /// <summary>
    /// Create User
    /// </summary>
    /// <param name="createUserRequest">User Dto</param>
    /// <param name="cancellationToken"></param>
    /// <remarks>
    /// Sample request:
    ///
    ///     POST api/users
    ///     {
    ///        "email": "john@gmail.com",
    ///        "firstName": "John",
    ///        "lastName": "White-Holland"
    ///     }
    /// </remarks>
    /// <response code="201">Returns created user</response>
    /// <returns>User</returns>
    [HasPermissions(Common.Permissions.Users.ReadWrite)]
    [HttpPost(Name = "CreateUser")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser(CreateUserRequest createUserRequest,
        CancellationToken cancellationToken)
    {
        var newUser = await _userService.CreateUserAsync(createUserRequest.Adapt<UserDto>(), cancellationToken);

        return Created($"/users/{newUser.Id}", newUser.Adapt<UserResponse>());
    }

    /// <summary>
    /// Endpoint to export users
    /// </summary>
    /// <param name="exportUsersRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Users.ReadExport)]
    [HttpGet("export", Name = "ExportUsers")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportUsersAsync([FromQuery] ExportUsersRequest exportUsersRequest,
        CancellationToken cancellationToken)
    {
        var users = await _userService.ExportUsersAsync(Guid.Empty, exportUsersRequest.FileType, cancellationToken);
        var mimeType = exportUsersRequest.FileType switch
        {
            FileType.Csv => "text/csv",
            FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            _ => throw new InvalidOperationException()
        };

        return File(users, mimeType, $"users.{exportUsersRequest.FileType.ToString().ToLowerInvariant()}");
    }

    /// <summary>
    /// Assign Role to User
    /// </summary>
    [HasPermissions(Common.Permissions.Users.RolesWrite)]
    [HttpPost("{id:guid}/assign", Name = "AssignRoles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> AssignRole([FromBody] Guid[] roleIds,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _userService.AssignRoleAsync(roleIds, id, cancellationToken);

        return Ok();
    }
}

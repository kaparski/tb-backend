using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Users.Requests;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.Administration.Users;
using TaxBeacon.Administration.Users.Models;

namespace TaxBeacon.API.Controllers.Users;

[Authorize]
public class UsersController: BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>
    /// Queryable list of users (as OData endpoint).
    /// </summary>
    /// <response code="200">Returns users</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of users</returns>
    [HasPermissions(
        Common.Permissions.Users.Read,
        Common.Permissions.Users.ReadWrite,
        Common.Permissions.Users.ReadExport)]
    [EnableQuery]
    [HttpGet("api/odata/users")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<UserResponse> Get()
    {
        // Note that this method's name is predefined

        var query = _userService.QueryUsers();

        return query.ProjectToType<UserResponse>();
    }

    /// <summary>
    /// List of users
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET /users?page=1&amp;pageSize=10&amp;orderBy=email%20desc&amp;filter=firstName%3DPeter```<br/><br/>
    ///     ```GET /users?page=2&amp;pageSize=5&amp;orderBy=email```
    /// </remarks>
    /// <response code="200">Returns users</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of users</returns>
    [HasPermissions(
        Common.Permissions.Users.Read,
        Common.Permissions.Users.ReadWrite,
        Common.Permissions.Users.ReadExport)]
    [HttpGet(Name = "GetUsers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetUserList([FromQuery] GridifyQuery query,
        CancellationToken cancellationToken)
    {
        if (!query.IsValid<UserDto>())
        {
            // TODO: Add an object with errors that we can use to detail the answers
            return BadRequest();
        }

        var users = await _userService.GetUsersAsync(query, cancellationToken);
        return Ok(new QueryablePaging<UserResponse>(users.Count, users.Query.ProjectToType<UserResponse>()));
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
    ///        "firstName": "Johny",
    ///        "legalName": "John",
    ///        "lastName": "White-Holland"
    ///     }
    /// </remarks>
    /// <response code="201">Returns created user</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="409">A user with such an email already exists</response>
    /// <returns>User</returns>
    [HasPermissions(Common.Permissions.Users.ReadWrite)]
    [HttpPost(Name = "CreateUser")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateUserAsync(CreateUserRequest createUserRequest,
        CancellationToken cancellationToken)
    {
        var createUserResult = await _userService
            .CreateUserAsync(createUserRequest.Adapt<CreateUserDto>(), cancellationToken);

        return createUserResult.Match<IActionResult>(
            newUser => Created($"/users/{newUser.Id}", newUser.Adapt<UserResponse>()),
            _ => Conflict(),
            error => BadRequest(error.Message));
    }

    /// <summary>
    /// Endpoint to export users
    /// </summary>
    /// <param name="exportUsersRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Users.ReadExport)]
    [HttpGet("export", Name = "ExportUsers")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportUsersAsync([FromQuery] ExportUsersRequest exportUsersRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportUsersRequest.FileType.ToMimeType();

        var users = await _userService.ExportUsersAsync(exportUsersRequest.FileType, cancellationToken);

        return File(users, mimeType, $"users.{exportUsersRequest.FileType.ToString().ToLowerInvariant()}");
    }
}

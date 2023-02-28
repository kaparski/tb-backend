using Gridify;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    [HttpGet(Name = "GetUsers")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<UserResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryablePaging<UserResponse>>> GetUserList([FromQuery] GridifyQuery query, CancellationToken cancellationToken)
    {
        var users = await _userService.GetUsersAsync(query, cancellationToken);
        var userListResponse =
            new QueryablePaging<UserResponse>(users.Count, users.Query.ProjectToType<UserResponse>());

        return Ok(userListResponse);
    }

    /// <summary>
    /// User Details
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET api/users/8da4f695-6d47-4ce8-da8f-08db0052f325```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns user details</response>
    /// <returns>User</returns>
    [HttpGet("{id:guid}", Name = "GetUserDetails")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserDetails([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userDto = await _userService.GetUserByIdAsync(id, cancellationToken);

        return Ok(userDto.Adapt<UserResponse>());
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
    [HttpPost(Name = "CreateUser")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateUser(CreateUserRequest createUserRequest, CancellationToken cancellationToken)
    {
        var newUser = await _userService.CreateUserAsync(createUserRequest.Adapt<UserDto>(), cancellationToken);

        return Created($"/users/{newUser.Id}", newUser.Adapt<UserResponse>());
    }

    /// <summary>
    /// Endpoint to update user status
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="userStatus">New user status</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns updated user</response>
    /// <response code="401">User is unauthorized</response>
    /// <returns>Updated user</returns>
    [HttpPut("{id:guid}/status", Name = "UpdateUserStatus")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> UpdateUserStatusAsync(Guid id, [FromBody] UserStatus userStatus,
        CancellationToken cancellationToken)
    {
        var user = await _userService.UpdateUserStatusAsync(id, userStatus, cancellationToken);

        return Ok(user.Adapt<UserResponse>());
    }
}

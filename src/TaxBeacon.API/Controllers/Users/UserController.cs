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
[Route("user/{id:guid}")]
public class UserController: BaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService) => _userService = userService;

    /// <summary>
    /// User Details
    /// </summary>
    /// <remarks>
    /// Sample requests: <br/><br/>
    ///     ```GET api/users/8da4f695-6d47-4ce8-da8f-08db0052f325```<br/><br/>
    /// </remarks>
    /// <response code="200">Returns user details</response>
    /// <returns>User</returns>
    [HasPermissions(
        Common.Permissions.Users.Read,
        Common.Permissions.Users.ReadWrite,
        Common.Permissions.Users.ReadExport)]
    [HttpGet(Name = "GetUserDetails")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserDetailsAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var userDto = await _userService.GetUserByIdAsync(id, cancellationToken);

        return Ok(userDto.Adapt<UserResponse>());
    }

    /// <summary>
    /// Endpoint to update user information
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="updateUserRequest">Updated user information</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Updated user</response>
    /// <response code="400">Invalid request</response>
    /// <response code="404">No user with this Id was found</response>
    /// <returns></returns>
    [HasPermissions(Common.Permissions.Users.ReadWrite)]
    [HttpPatch(Name = "UpdateUser")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BadRequest), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(NotFound), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserAsync([FromRoute] Guid id,
        [FromBody] UpdateUserRequest updateUserRequest,
        CancellationToken cancellationToken)
    {
        var updatedUserOneOf =
            await _userService.UpdateUserByIdAsync(Guid.Parse(HttpContext!.User!.FindFirst(Claims.TenantId)!.Value),
                id,
                updateUserRequest.Adapt<UpdateUserDto>(),
                cancellationToken);

        return updatedUserOneOf.Match<IActionResult>(
            user => Ok(user.Adapt<UserResponse>()),
            notFound => NotFound());
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
    [HasPermissions(Common.Permissions.Users.ReadWrite)]
    [HttpPut("status", Name = "UpdateUserStatus")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<UserResponse>> UpdateUserStatusAsync(Guid id, [FromBody] Status userStatus,
        CancellationToken cancellationToken)
    {

        var user = await _userService.UpdateUserStatusAsync(
            Guid.Parse(HttpContext!.User!.FindFirst(Claims.TenantId)!.Value), id, userStatus, cancellationToken);

        return Ok(user.Adapt<UserResponse>());
    }
}

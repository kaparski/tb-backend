using Mapster;
using Microsoft.AspNetCore.Authorization;
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
[Route("api/[controller]/{id:guid}")]
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
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">No user with this Id was found</response>
    /// <returns>User details</returns>
    [HasPermissions(
        Common.Permissions.Users.Read,
        Common.Permissions.Users.ReadWrite,
        Common.Permissions.Users.ReadExport)]
    [HttpGet(Name = "GetUserDetails")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetUserDetailsAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var getUserDetailsResult = await _userService.GetUserDetailsByIdAsync(id, cancellationToken);

        return getUserDetailsResult.Match<IActionResult>(
            user => Ok(user.Adapt<UserResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Endpoint to update user information
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="updateUserRequest">Updated user information</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Updated user</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">No user with this Id was found</response>
    /// <returns></returns>
    [HasPermissions(Common.Permissions.Users.ReadWrite)]
    [HttpPatch(Name = "UpdateUser")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserAsync([FromRoute] Guid id,
        [FromBody] UpdateUserRequest updateUserRequest,
        CancellationToken cancellationToken)
    {
        var updatedUserResult =
            await _userService.UpdateUserByIdAsync(id,
                updateUserRequest.Adapt<UpdateUserDto>(),
                cancellationToken);

        return updatedUserResult.Match<IActionResult>(
            user => Ok(user.Adapt<UserResponse>()),
            _ => NotFound(),
            error => BadRequest(error.Message));
    }

    /// <summary>
    /// Endpoint to update user status
    /// </summary>
    /// <param name="id">User id</param>
    /// <param name="userStatus">New user status</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns updated user</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">No user with this Id was found</response>
    /// <returns>Updated user</returns>
    [HasPermissions(Common.Permissions.Users.ReadWrite)]
    [HttpPut("status", Name = "UpdateUserStatus")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateUserStatusAsync(Guid id, [FromBody] Status userStatus,
        CancellationToken cancellationToken)
    {
        var updatedUserStatusResult = await _userService.UpdateUserStatusAsync(id, userStatus, cancellationToken);

        return updatedUserStatusResult.Match<IActionResult>(
            user => Ok(user.Adapt<UserResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Assign Role to User
    /// </summary>
    /// <response code="200">Roles have been successfully assigned to a user</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">No user with this Id was found</response>
    /// <returns>Success response</returns>
    [HasPermissions(Common.Permissions.Users.ReadWrite)]
    [HttpPost("assign", Name = "AssignRoles")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole([FromBody] Guid[] roleIds,
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var changeUserRolesResult = await _userService.ChangeUserRolesAsync(id, roleIds, cancellationToken);

        return changeUserRolesResult.Match<IActionResult>(
           _ => Ok(),
           _ => NotFound());
    }

    /// <summary>
    /// Get User Activity History
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">No user with this Id was found</response>
    /// <returns>Activity history for a specific department</returns>
    [HasPermissions(Common.Permissions.Users.Read)]
    [HttpGet("activities", Name = "UserActivityHistory")]
    [ProducesResponseType(typeof(IEnumerable<UserActivityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UserActivitiesHistory([FromRoute] Guid id,
        [FromQuery] UserActivitiesRequest request,
        CancellationToken cancellationToken)
    {
        var activities = await _userService.GetActivitiesAsync(id, request.Page, request.PageSize, cancellationToken);

        return activities.Match<IActionResult>(
            result => Ok(result.Adapt<UserActivityResponse>()),
            _ => NotFound());
    }
}

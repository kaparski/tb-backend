using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Users;

[Authorize]
public class UserProfileController: BaseController
{
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUserService;

    public UserProfileController(IUserService userService, ICurrentUserService currentUserService)
    {
        _userService = userService;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// My Profile
    /// </summary>
    /// <response code="200">Returns my profile details</response>
    /// <response code="401">User is unauthorized</response>
    /// <returns>User</returns>
    [HttpGet("/api/me/profile", Name = "GetMyProfile")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var userDto = await _userService.GetUserDetailsByIdAsync(currentUserId, cancellationToken);

        return Ok(userDto.Adapt<UserResponse>());
    }
}

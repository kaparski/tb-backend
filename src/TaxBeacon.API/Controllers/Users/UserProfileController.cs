using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.Common.Services;
using TaxBeacon.Administration.Users;

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
    /// <response code="404">User was not found</response>
    /// <returns>User</returns>
    [HttpGet("/api/me/profile", Name = "GetMyProfile")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
    {
        var getUserDetailsResult = await _userService.GetUserProfileAsync(cancellationToken);

        return getUserDetailsResult.Match<IActionResult>(
            userDto => Ok(userDto.Adapt<UserResponse>()),
            _ => NotFound());
    }
}

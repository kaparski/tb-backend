using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using TaxBeacon.API.Controllers.Authorization.Requests;
using TaxBeacon.API.Controllers.Authorization.Responses;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Authorization
{
    [Authorize]
    public class AuthorizationController: BaseController
    {
        private readonly IUserService _userService;
        private readonly IPermissionsService _permissionsService;

        public AuthorizationController(
            IUserService userService,
            IPermissionsService permissionsService)
        {
            _userService = userService;
            _permissionsService = permissionsService;
        }

        /// <summary>
        /// Endpoint to save user last login date
        /// </summary>
        /// <param name="loginRequest">Request containing the user's email</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns success response</returns>
        /// <response code="200">User email is valid and last login date successfully saved</response>
        /// <response code="400">User email is invalid</response>
        /// <response code="401">User is unauthorized</response>
        [HttpPost("login", Name = "Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest,
            CancellationToken cancellationToken)
        {
            var userOneOf = await _userService.LoginAsync(new MailAddress(loginRequest.Email), cancellationToken);

            return await userOneOf.Match<Task<IActionResult>>(
                async user => Ok(
                    new LoginResponse(
                        user.Id,
                        user.FullName,
                        await _permissionsService.GetPermissionsAsync(user.Id, cancellationToken))),
                _ => Task.FromResult<IActionResult>(NotFound()));
        }
    }
}

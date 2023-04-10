using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using TaxBeacon.API.Controllers.Authorization.Requests;
using TaxBeacon.API.Controllers.Authorization.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Authorization
{
    [Authorize]
    public class AuthorizationController: BaseController
    {
        private readonly IUserService _userService;

        public AuthorizationController(IUserService userService) => _userService = userService;

        /// <summary>
        /// Endpoint to save user last login date
        /// </summary>
        /// <param name="loginRequest">Request containing the user's email</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns success response</returns>
        /// <response code="200">User email is valid and last login date successfully saved</response>
        /// <response code="400">User email is invalid</response>
        /// <response code="401">User is unauthorized</response>
        /// <response code="404">No user with this email was found</response>
        [HttpPost("login", Name = "Login")]
        [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest,
            CancellationToken cancellationToken)
        {
            var userOneOf = await _userService.LoginAsync(new MailAddress(loginRequest.Email), cancellationToken);

            return userOneOf.Match<IActionResult>(
                user => Ok(user.Adapt<LoginResponse>()),
                _ => NotFound());
        }
    }
}

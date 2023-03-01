using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using TaxBeacon.API.Controllers.Authorization.Requests;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Authorization
{
    [Authorize]
    public class AuthorizationController: BaseController
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IUserService _userService;

        public AuthorizationController(
            ILogger<AuthorizationController> logger,
            IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        /// <summary>
        /// Endpoint to save user last login date
        /// </summary>
        /// <param name="loginRequest">Request containing the user's email</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Returns success response</returns>
        /// <response code="200">User email valid and last login date successfully saved</response>
        /// <response code="400">User email invalid</response>
        /// <response code="401">User is unauthorized</response>
        [HttpPost("login", Name = "Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest,
            CancellationToken cancellationToken)
        {
            await _userService.LoginAsync(new MailAddress(loginRequest.Email), cancellationToken);

            return Ok();
        }
    }
}

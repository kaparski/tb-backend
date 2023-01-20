using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Authorization
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AuthorizationController: ControllerBase
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

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequest loginRequest,
            CancellationToken cancellationToken)
        {
            await _userService.LoginAsync(loginRequest.Email, cancellationToken);

            return Ok();
        }
    }
}

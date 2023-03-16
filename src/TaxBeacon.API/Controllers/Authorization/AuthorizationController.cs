using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Authorization.Requests;
using TaxBeacon.API.Controllers.Authorization.Responses;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Authorization
{
    [Authorize]
    public class AuthorizationController: BaseController
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IUserService _userService;
        private readonly IPermissionsService _permissionsService;

        public AuthorizationController(
            ILogger<AuthorizationController> logger,
            IUserService userService,
            IPermissionsService permissionsService)
        {
            _logger = logger;
            _userService = userService;
            _permissionsService = permissionsService;
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
        public async Task<ActionResult<LoginResponse>> LoginAsync([FromBody] LoginRequest loginRequest,
            CancellationToken cancellationToken)
        {
            await _userService.LoginAsync(new MailAddress(loginRequest.Email), cancellationToken);

            var successUserIdParse = Guid.TryParse(
                Request?.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type == Claims.UserIdClaimName)?.Value,
                out var userId);
            var permissions = Guid.TryParse(Request?.HttpContext?.User?.Claims?.FirstOrDefault(c => c.Type == Claims.TenantId)?.Value, out var tenantId) &&
                              successUserIdParse
                              ? await _permissionsService.GetPermissionsAsync(tenantId, userId) : Array.Empty<string>();

            return Ok(new LoginResponse
            {
                UserId = userId,
                Permissions = permissions
            });
        }
    }
}

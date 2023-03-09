using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Security.Claims;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Authentication
{
    public sealed class PermissionsAuthorizationHandler: AuthorizationHandler<PermissionsRequirement>
    {
        private const string UserIdClaimName = "userId";
        private const string EmailClaimName = "preferred_username";
        private const string TenantId = "tenantId";
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PermissionsAuthorizationHandler> _logger;

        public PermissionsAuthorizationHandler(IServiceScopeFactory scopeFactory, ILogger<PermissionsAuthorizationHandler> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionsRequirement requirement)
        {

            var email = context.User.Claims
                .FirstOrDefault(claim => claim.Type.Equals(EmailClaimName, StringComparison.OrdinalIgnoreCase))
                ?.Value;

            if (email is null)
            {
                return;
            }

            using var scope = _scopeFactory.CreateScope();

            try
            {
                var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
                var user = await userService.GetUserByEmailAsync(new MailAddress(email), default);
                var tenantId = await userService.GetTenantIdAsync(user.Id);

                if (user.DeactivationDateTimeUtc is not null || user.UserStatus == UserStatus.Deactivated)
                {
                    context.Fail();
                    return;
                }

                if (!context.User.HasClaim(claim =>
                        claim.Type.Equals(UserIdClaimName, StringComparison.OrdinalIgnoreCase)))
                {
                    var claimsIdentity = new ClaimsIdentity();
                    claimsIdentity.AddClaim(new Claim(UserIdClaimName, user.Id.ToString()));
                    claimsIdentity.AddClaim(new Claim(TenantId, tenantId.ToString()));
                    context.User.AddIdentity(claimsIdentity);
                }

                var permissionsService = scope.ServiceProvider.GetRequiredService<IPermissionsService>();

                IEnumerable<string> permissions = await permissionsService.GetPermissionsAsync(tenantId, user.Id);

                if (permissions.Intersect(requirement.Permissions).Any())
                {
                    context.Succeed(requirement);
                }
            }
            catch (NotFoundException ex)
            {
                context.Fail();
                _logger.LogError(ex, "Failed to authenticate user with {@email}", email);
            }
        }
    }
}

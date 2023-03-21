using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Authentication
{
    public sealed class PermissionsAuthorizationHandler: AuthorizationHandler<PermissionsRequirement>
    {
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
                .FirstOrDefault(claim => claim.Type.Equals(Claims.EmailClaimName, StringComparison.OrdinalIgnoreCase))
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

                if (user.DeactivationDateTimeUtc is not null || user.Status == Status.Deactivated)
                {
                    context.Fail();
                    return;
                }

                var permissionsService = scope.ServiceProvider.GetRequiredService<IPermissionsService>();

                var tenantIdClaim = context.User.Claims
                    .FirstOrDefault(claim => claim.Type.Equals(Claims.TenantId, StringComparison.OrdinalIgnoreCase))
                    ?.Value;

                if (!Guid.TryParse(tenantIdClaim, out var tenantId))
                {
                    return;
                }

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

using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Extensions;

public static class UserIQueryableExtension
{
    // TODO: Fix editor config to avoid pragma and create a materialized view for Roles
#pragma warning disable IDE0055
    public static IQueryable<UserDto> MapToUserDto(this IQueryable<User> source, ITaxBeaconDbContext context,
        ICurrentUserService currentUserService) =>
        source.Select(user => new
            {
                User = user,
                Roles = context.TenantUserRoles
                    .Where(tur => tur.UserId == user.Id && tur.TenantId == currentUserService.TenantId)
                    .Join(context.Roles, tur => tur.RoleId, r => r.Id, (tur, r) => r.Name)
                    .GroupBy(key => 1, name => name)
                    .Select(group => string.Join(", ", group.Select(name => name)))
                    .FirstOrDefault()
            })
            .Select(userWithRoles => new UserDto
            {
                Id = userWithRoles.User.Id,
                FirstName = userWithRoles.User.FirstName,
                LastName = userWithRoles.User.LastName,
                CreatedDateTimeUtc = userWithRoles.User.CreatedDateTimeUtc,
                Email = userWithRoles.User.Email,
                Status = userWithRoles.User.Status,
                LastLoginDateTimeUtc = userWithRoles.User.LastLoginDateTimeUtc,
                FullName = userWithRoles.User.FullName,
                DeactivationDateTimeUtc = userWithRoles.User.DeactivationDateTimeUtc,
                ReactivationDateTimeUtc = userWithRoles.User.ReactivationDateTimeUtc,
#pragma warning disable IDE0029
                Roles = userWithRoles.Roles != null ? userWithRoles.Roles : string.Empty
#pragma warning restore IDE0029
            });
#pragma warning restore IDE0055

}

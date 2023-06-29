using TaxBeacon.Administration.Users.Models;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.Users.Extensions;

public static class UserIQueryableExtension
{
    public static IQueryable<UserDto> MapToUserDtoWithNoTenantRoleNames(this IQueryable<User> source,
        ITaxBeaconDbContext context) =>
        source
            .GroupJoin(context.UserRoles,
            u => new { u.Id },
            tur => new { Id = tur.UserId },
            (u, tur) => new { User = u, UserRoles = tur })
            .SelectMany(q => q.UserRoles.DefaultIfEmpty(),
            (u, tur) => new UserWithRoleId { User = u.User, RoleId = tur!.RoleId })
            .MapToUserDtoWithRoleNames(context);

    public static IQueryable<UserDto> MapToUserDtoWithTenantRoleNames(this IQueryable<User> source,
        ITaxBeaconDbContext context,
        ICurrentUserService currentUserService) =>
        source
            .GroupJoin(context.TenantUserRoles,
                u => new { u.Id, currentUserService.TenantId },
                tur => new { Id = tur.UserId, tur.TenantId },
                (u, tur) => new { User = u, TenantUserRole = tur })
            .SelectMany(q => q.TenantUserRole.DefaultIfEmpty(),
                (u, tur) => new UserWithRoleId { User = u.User, RoleId = tur!.RoleId })
            .MapToUserDtoWithRoleNames(context);

    private static IQueryable<UserDto> MapToUserDtoWithRoleNames(this IQueryable<UserWithRoleId> source,
        ITaxBeaconDbContext context) =>
        source
            .GroupJoin(context.Roles.OrderBy(r => r.Name),
             ur => new { ur.RoleId },
             role => new { RoleId = (Guid?)role.Id },
             (ur, roles) => new { ur.User, Roles = roles })
            .SelectMany(ur => ur.Roles.DefaultIfEmpty(),
             (ur, role) => new { ur.User, RoleName = "|" + role!.Name + "|" })
            .GroupBy(z => z.User)
            .Select(group => new UserDto
            {
                Id = group.Key.Id,
                FirstName = group.Key.FirstName,
                LegalName = group.Key.LegalName,
                LastName = group.Key.LastName,
                CreatedDateTimeUtc = group.Key.CreatedDateTimeUtc,
                Email = group.Key.Email,
                Status = group.Key.Status,
                LastLoginDateTimeUtc = group.Key.LastLoginDateTimeUtc,
                FullName = group.Key.FullName,
                DeactivationDateTimeUtc = group.Key.DeactivationDateTimeUtc,
                ReactivationDateTimeUtc = group.Key.ReactivationDateTimeUtc,
                Roles = string.Join(", ", group.Select(c => c.RoleName)),
            });

    private class UserWithRoleId
    {
        public User User { get; init; } = null!;

        public Guid? RoleId { get; init; }
    }
}

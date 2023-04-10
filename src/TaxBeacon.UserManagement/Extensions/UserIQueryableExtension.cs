using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Extensions;

public static class UserIQueryableExtension
{
    public static IQueryable<UserDto> MapToUserDto(this IQueryable<User> source, ITaxBeaconDbContext context,
        ICurrentUserService currentUserService) =>
        source
            .GroupJoin(context.TenantUserRoles,
                u => new { u.Id, currentUserService.TenantId },
                tur => new { Id = tur.UserId, tur.TenantId },
                (u, tur) => new { User = u, TenantUserRole = tur })
            .SelectMany(q => q.TenantUserRole.DefaultIfEmpty(),
                (u, tur) => new { u.User, RoleId = (Guid?)tur!.RoleId })
            .GroupJoin(context.Roles,
                ur => new { ur.RoleId },
                role => new { RoleId = (Guid?)role.Id },
                (ur, roles) => new { ur.User, Roles = roles })
            .SelectMany(ur => ur.Roles.DefaultIfEmpty(),
                (ur, role) => new { ur.User, role!.Name })
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
                Roles = string.Join(", ", group.Select(c => c.Name))
            });
}

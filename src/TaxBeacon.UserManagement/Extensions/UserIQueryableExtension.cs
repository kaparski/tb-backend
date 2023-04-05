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
                (u, tur) => new { u.User, tur.RoleId })
            .GroupJoin(context.Roles,
                a => new { a.RoleId },
                y => new { RoleId = y.Id },
                (a, y) => new { a, y })
            .SelectMany(q => q.y.DefaultIfEmpty(),
                (a, y) => new { a.a.User, y.Name })
            .GroupBy(z => z.User)
            .Select(group => new UserDto
            {
                Id = group.Key.Id,
                FirstName = group.Key.FirstName,
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

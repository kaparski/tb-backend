using Gridify;
using Gridify.EntityFramework;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Text.Json;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities;

namespace TaxBeacon.UserManagement.Services;

public class RoleService: IRoleService
{
    private readonly ITaxBeaconDbContext _context;
    private readonly ILogger<RoleService> _logger;
    private readonly IDateTimeService _dateTimeService;
    private readonly ICurrentUserService _currentUserService;

    public RoleService(ITaxBeaconDbContext context,
        ILogger<RoleService> logger,
        IDateTimeService dateTimeService,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
    }

    public async Task<QueryablePaging<RoleDto>> GetRolesAsync(IGridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        _currentUserService.TenantId != default
            ? await GetTenantRolesAsync(gridifyQuery, cancellationToken)
            : await GetNotTenantRolesAsync(gridifyQuery, cancellationToken);

    public async Task<OneOf<QueryablePaging<RoleAssignedUserDto>, NotFound>> GetRoleAssignedUsersAsync(Guid roleId,
        IGridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        var usersResult = _currentUserService.TenantId != default
            ? await GetTenantRoleAssignedUsersAsync(roleId, cancellationToken)
            : await GetNotTenantRoleAssignedUsersAsync(roleId, cancellationToken);

        return await usersResult.Match<Task<OneOf<QueryablePaging<RoleAssignedUserDto>, NotFound>>>(
            async users =>
                await users.ProjectToType<RoleAssignedUserDto>()
                    .GridifyQueryableAsync(gridifyQuery, null, cancellationToken),
            notFound => Task.FromResult<OneOf<QueryablePaging<RoleAssignedUserDto>, NotFound>>(notFound));
    }

    public async Task<OneOf<Success, NotFound>> UnassignUsersAsync(Guid roleId,
        List<Guid> users,
        CancellationToken cancellationToken)
    {
        var unassignResult = _currentUserService.TenantId != default
            ? await UnassignTenantUsersAsync(roleId, users, cancellationToken)
            : await UnassignNotTenantUsersAsync(roleId, users, cancellationToken);

        if (!unassignResult.TryPickT0(out var role, out var notFound))
        {
            return notFound;
        }

        var userInfo = _currentUserService.UserInfo;
        var activityUserLogs = users
            .Select(x => new UserActivityLog
            {
                TenantId = _currentUserService.TenantId,
                UserId = x,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new UnassignRolesEvent(
                        role.Name,
                        _dateTimeService.UtcNow,
                        _currentUserService.UserId,
                        userInfo.FullName,
                        userInfo.Roles
                    )),
                EventType = UserEventType.UserRolesUnassign
            });

        await _context.UserActivityLogs.AddRangeAsync(activityUserLogs, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Users({userIds}) were unassigned from role({roleId}) by {userId}",
            _dateTimeService.UtcNow,
            string.Join(",", users),
            roleId,
            _currentUserService.UserId);

        return new Success();
    }

    public async Task<OneOf<Success, NotFound>> AssignUsersAsync(Guid roleId,
        List<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var assignResult = _currentUserService.TenantId != default
            ? await AssignTenantUsersAsync(roleId, userIds, cancellationToken)
            : await AssignNotTenantUsersAsync(roleId, userIds, cancellationToken);

        if (!assignResult.TryPickT0(out var role, out var notFound))
        {
            return notFound;
        }

        var eventDateTime = _dateTimeService.UtcNow;
        var userInfo = _currentUserService.UserInfo;

        var activityLogs = userIds.Select(userId => new UserActivityLog
        {
            TenantId = _currentUserService.TenantId,
            UserId = userId,
            Date = eventDateTime,
            Revision = 1,
            Event = JsonSerializer.Serialize(
                new AssignRolesEvent(
                    role.Name,
                    eventDateTime,
                    _currentUserService.UserId,
                    userInfo.FullName,
                    userInfo.Roles)),
            EventType = UserEventType.UserRolesAssign
        });

        await _context.UserActivityLogs.AddRangeAsync(activityLogs, cancellationToken);

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Users({userIds}) have been assigned to the role({roleId}) by {userId}",
            _dateTimeService.UtcNow,
            string.Join(',', userIds),
            roleId,
            _currentUserService.UserId);

        return new Success();
    }

    public async Task<IReadOnlyCollection<PermissionDto>> GetRolePermissionsByIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var permissions =
            _currentUserService.TenantId != default
                ? await GetTenantRolePermissionsByIdAsync(roleId, cancellationToken)
                : await GetNotTenantRolePermissionsByIdAsync(roleId, cancellationToken);

        var permissionsWithCategory = permissions.Select(p => p with
        {
            Category = p.Name.Split('.')[0]
        }).ToList();

        return permissionsWithCategory;
    }

    private Task<QueryablePaging<RoleDto>> GetTenantRolesAsync(IGridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        _context.TenantRoles
            .AsNoTracking()
            .Where(tr => tr.TenantId == _currentUserService.TenantId)
            .Select(tr => new RoleDto
            {
                Id = tr.RoleId,
                Name = tr.Role.Name,
                AssignedUsersCount = tr.TenantUserRoles.Count
            })
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

    private Task<QueryablePaging<RoleDto>> GetNotTenantRolesAsync(IGridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        _context.Roles
            .AsNoTracking()
            .Where(r => r.Type == SourceType.System)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                AssignedUsersCount = r.UserRoles.Count
            })
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

    private async Task<OneOf<IQueryable<User>, NotFound>> GetTenantRoleAssignedUsersAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var roleExists = await _context.Roles
            .AnyAsync(
                r => r.Id == roleId && r.TenantRoles.Any(tr => tr.TenantId == _currentUserService.TenantId),
                cancellationToken);

        return !roleExists
            ? new NotFound()
            : OneOf<IQueryable<User>, NotFound>.FromT0(
                _context.TenantUserRoles
                    .Where(tr => tr.TenantId == _currentUserService.TenantId && tr.RoleId == roleId)
                    .Select(tr => tr.TenantUser.User));
    }

    private async Task<OneOf<IQueryable<User>, NotFound>> GetNotTenantRoleAssignedUsersAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var roleExists = await _context.Roles.AnyAsync(r => r.Id == roleId, cancellationToken);

        return !roleExists
            ? new NotFound()
            : OneOf<IQueryable<User>, NotFound>.FromT0(
                _context.UserRoles
                    .Where(ur => ur.RoleId == roleId)
                    .Select(ur => ur.User));
    }

    private async Task<OneOf<Role, NotFound>> UnassignTenantUsersAsync(Guid roleId,
        IEnumerable<Guid> users,
        CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .Where(r => r.Id == roleId
                        && r.TenantRoles.Any(tr => tr.TenantId == _currentUserService.TenantId))
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null)
        {
            return new NotFound();
        }

        var usersToRemove = _context.TenantUserRoles
            .Where(x => x.TenantId == _currentUserService.TenantId && x.RoleId == roleId && users.Contains(x.UserId));

        _context.TenantUserRoles.RemoveRange(usersToRemove);

        return role;
    }

    private async Task<OneOf<Role, NotFound>> UnassignNotTenantUsersAsync(Guid roleId,
        IEnumerable<Guid> users,
        CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .Where(r => r.Id == roleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null)
        {
            return new NotFound();
        }

        var usersToRemove = _context.UserRoles
            .Where(x => x.RoleId == roleId && users.Contains(x.UserId));

        _context.UserRoles.RemoveRange(usersToRemove);

        return role;
    }

    private async Task<OneOf<Role, NotFound>> AssignTenantUsersAsync(Guid roleId,
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var role = await _context.TenantRoles
            .Where(tr => tr.TenantId == _currentUserService.TenantId && tr.RoleId == roleId)
            .Select(tr => tr.Role)
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null)
        {
            return new NotFound();
        }

        var tenantUserRolesToAdd = userIds.Select(userId => new TenantUserRole
        {
            TenantId = _currentUserService.TenantId,
            RoleId = roleId,
            UserId = userId
        });

        await _context.TenantUserRoles.AddRangeAsync(tenantUserRolesToAdd, cancellationToken);

        return role;
    }

    private async Task<OneOf<Role, NotFound>> AssignNotTenantUsersAsync(Guid roleId,
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default)
    {
        var role = await _context.Roles
            .Where(r => r.Id == roleId)
            .FirstOrDefaultAsync(cancellationToken);

        if (role is null)
        {
            return new NotFound();
        }

        var tenantUserRolesToAdd = userIds.Select(userId => new UserRole { RoleId = roleId, UserId = userId });

        await _context.UserRoles.AddRangeAsync(tenantUserRolesToAdd, cancellationToken);

        return role;
    }

    private async Task<List<PermissionDto>> GetTenantRolePermissionsByIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default) =>
        await _context.TenantRolePermissions
            .AsNoTracking()
            .Where(trp => trp.TenantId == _currentUserService.TenantId && trp.RoleId == roleId)
            .Join(_context.Permissions,
                trp => trp.PermissionId,
                p => p.Id,
                (trp, p) => new { p.Id, p.Name })
            .ProjectToType<PermissionDto>()
            .ToListAsync(cancellationToken);

    private async Task<List<PermissionDto>> GetNotTenantRolePermissionsByIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default) =>
        await _context.RolePermissions
            .AsNoTracking()
            .Where(rp => rp.RoleId == roleId)
            .Join(_context.Permissions,
                trp => trp.PermissionId,
                p => p.Id,
                (trp, p) => new { p.Id, p.Name })
            .ProjectToType<PermissionDto>()
            .ToListAsync(cancellationToken);
}

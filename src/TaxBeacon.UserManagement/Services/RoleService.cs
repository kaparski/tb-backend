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
        CancellationToken cancellationToken = default)
    {
        var roles = _currentUserService.TenantId != default
            ? GetTenantRolesQuery()
            : GetNotTenantRolesQuery();

        return await roles.GridifyQueryableAsync(gridifyQuery, null, cancellationToken);
    }

    public async Task<OneOf<QueryablePaging<RoleAssignedUserDto>, NotFound>> GetRoleAssignedUsersAsync(Guid roleId,
        IGridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        if (!await IsRoleExistsAsync(roleId, cancellationToken))
        {
            return new NotFound();
        }

        var users = _currentUserService.TenantId != default
            ? GetTenantRoleAssignedUsersQuery(roleId)
            : GetNotTenantRoleAssignedUsersQuery(roleId);

        return await users
            .ProjectToType<RoleAssignedUserDto>()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);
    }

    public async Task<OneOf<Success, NotFound>> UnassignUsersAsync(Guid roleId,
        List<Guid> users,
        CancellationToken cancellationToken)
    {
        var getRoleResult = await GetRoleByIdAsync(roleId, cancellationToken);

        if (!getRoleResult.TryPickT0(out var role, out var notFound))
        {
            return notFound;
        }

        if (_currentUserService.TenantId == default)
        {
            UnassignNotTenantUsers(roleId, users);
        }
        else
        {
            UnassignTenantUsers(roleId, users);
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
        var getRoleResult = await GetRoleByIdAsync(roleId, cancellationToken);

        if (!getRoleResult.TryPickT0(out var role, out var notFound))
        {
            return notFound;
        }

        if (_currentUserService.TenantId == default)
        {
            await AssignNotTenantUsersAsync(roleId, userIds, cancellationToken);
        }
        else
        {
            await AssignTenantUsersAsync(roleId, userIds, cancellationToken);
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

    public async Task<OneOf<IReadOnlyCollection<PermissionDto>, NotFound>> GetRolePermissionsByIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        if (!await IsRoleExistsAsync(roleId, cancellationToken))
        {
            return new NotFound();
        }

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

    private async Task<OneOf<Role, NotFound>> GetRoleByIdAsync(Guid roleId, CancellationToken cancellationToken)
    {
        var role = _currentUserService.TenantId == default
            ? await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId, cancellationToken)
            : await _context.Roles
                .FirstOrDefaultAsync(r => r.Id == roleId
                                          && r.TenantRoles.Any(tr => tr.TenantId == _currentUserService.TenantId),
                    cancellationToken);

        return role is not null ? role : new NotFound();
    }

    private async Task<bool> IsRoleExistsAsync(Guid roleId, CancellationToken cancellationToken) =>
        _currentUserService.TenantId == default
            ? await _context.Roles.AnyAsync(r => r.Id == roleId, cancellationToken)
            : await _context.Roles
                .AnyAsync(r => r.Id == roleId && r.TenantRoles.Any(tr => tr.TenantId == _currentUserService.TenantId),
                    cancellationToken);

    private IQueryable<RoleDto> GetTenantRolesQuery() =>
        _context.TenantRoles
            .AsNoTracking()
            .Where(tr => tr.TenantId == _currentUserService.TenantId)
            .Select(tr => new RoleDto
            {
                Id = tr.RoleId,
                Name = tr.Role.Name,
                AssignedUsersCount = tr.TenantUserRoles.Count
            });

    private IQueryable<RoleDto> GetNotTenantRolesQuery() =>
        _context.Roles
            .AsNoTracking()
            .Where(r => r.Type == SourceType.System)
            .Select(r => new RoleDto
            {
                Id = r.Id,
                Name = r.Name,
                AssignedUsersCount = r.UserRoles.Count
            });

    private IQueryable<User> GetTenantRoleAssignedUsersQuery(Guid roleId) =>
        _context.TenantUserRoles
            .Where(tr => tr.TenantId == _currentUserService.TenantId && tr.RoleId == roleId)
            .Select(tr => tr.TenantUser.User);

    private IQueryable<User> GetNotTenantRoleAssignedUsersQuery(Guid roleId) =>
        _context.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.User);

    private void UnassignTenantUsers(Guid roleId, IEnumerable<Guid> users)
    {
        var usersToRemove = _context.TenantUserRoles
            .Where(x => x.TenantId == _currentUserService.TenantId && x.RoleId == roleId && users.Contains(x.UserId));

        _context.TenantUserRoles.RemoveRange(usersToRemove);
    }

    private void UnassignNotTenantUsers(Guid roleId, IEnumerable<Guid> users)
    {
        var usersToRemove = _context.UserRoles
            .Where(x => x.RoleId == roleId && users.Contains(x.UserId));

        _context.UserRoles.RemoveRange(usersToRemove);
    }

    private async Task AssignTenantUsersAsync(Guid roleId,
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default) =>
        await _context.TenantUserRoles
            .AddRangeAsync(userIds
                .Select(userId => new TenantUserRole
                {
                    TenantId = _currentUserService.TenantId,
                    RoleId = roleId,
                    UserId = userId
                }), cancellationToken);

    private async Task AssignNotTenantUsersAsync(Guid roleId,
        IEnumerable<Guid> userIds,
        CancellationToken cancellationToken = default) =>
        await _context.UserRoles
            .AddRangeAsync(userIds
                .Select(userId => new UserRole
                {
                    RoleId = roleId,
                    UserId = userId
                }), cancellationToken);

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

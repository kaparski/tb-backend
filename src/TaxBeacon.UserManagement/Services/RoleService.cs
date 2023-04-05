﻿using Gridify;
using Gridify.EntityFramework;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using TaxBeacon.Common.Services;
using System.Text.Json;
using TaxBeacon.Common.Enums.Activities;
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

    public RoleService(ITaxBeaconDbContext context, ILogger<RoleService> logger, IDateTimeService dateTimeService, ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
    }

    public Task<QueryablePaging<RoleDto>> GetRolesAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default) =>
        _context.TenantRoles
            .Where(tr => tr.TenantId == _currentUserService.TenantId)
            .Select(tr => new RoleDto
            {
                Id = tr.RoleId,
                Name = tr.Role.Name,
                AssignedUsersCount = tr.TenantUserRoles.Count
            })
            .AsNoTracking()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

    public async Task<OneOf<QueryablePaging<UserDto>, NotFound>> GetRoleAssignedUsersAsync(Guid roleId, GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        var roleExists = await _context.Roles
            .AnyAsync(
                r => r.Id == roleId && r.TenantRoles.Any(tr => tr.TenantId == _currentUserService.TenantId),
                cancellationToken);

        if (!roleExists)
        {
            return new NotFound();
        }

        var users = await _context.TenantUserRoles
            .Where(tr => tr.TenantId == _currentUserService.TenantId && tr.RoleId == roleId)
            .Select(tr => tr.TenantUser.User)
            .ProjectToType<UserDto>()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);

        return gridifyQuery.Page != 1 && gridifyQuery.Page > Math.Ceiling((double)users.Count / gridifyQuery.PageSize)
            ? new NotFound()
            : users;
    }

    public async Task<OneOf<Success, NotFound>> UnassignUsersAsync(Guid roleId, List<Guid> users, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        var tenantId = _currentUserService.TenantId;
        var role = await _context.Roles
            .Where(x => x.Id == roleId && x.TenantRoles.Select(x => x.TenantId).Contains(tenantId))
            .FirstOrDefaultAsync(cancellationToken);
        if (role is null)
        {
            return new NotFound();
        }

        var currentUser = await _context.Users
            .Where(x => x.Id == currentUserId && x.TenantUsers.Select(x => x.TenantId).Contains(tenantId))
            .FirstAsync(cancellationToken);
        var usersToRemove = _context.TenantUserRoles
            .Where(x => x.TenantId == tenantId && x.RoleId == roleId && users.Contains(x.UserId));

        _context.TenantUserRoles.RemoveRange(usersToRemove);

        var activityUserLogs = users
            .Select(x => new UserActivityLog
            {
                TenantId = tenantId,
                UserId = x,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new UnassignUsersEvent(
                        role.Name,
                        currentUserId,
                        currentUser.FullName,
                        _dateTimeService.UtcNow
                    )),
                EventType = EventType.UserRolesUnassign
            });
        await _context.UserActivityLogs
            .AddRangeAsync(activityUserLogs, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Users({userIds}) were unassigned from role({roleId}) by {userId}",
            _dateTimeService.UtcNow,
            string.Join(",", users),
            roleId,
            currentUserId);

        return new Success();
    }
}

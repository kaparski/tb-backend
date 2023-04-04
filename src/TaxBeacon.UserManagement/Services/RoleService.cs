using Gridify;
using Gridify.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Text.Json;
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

    public RoleService(ITaxBeaconDbContext context, ILogger<RoleService> logger, IDateTimeService dateTimeService, ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _dateTimeService = dateTimeService;
        _currentUserService = currentUserService;
    }

    public async Task<QueryablePaging<RoleDto>> GetRolesAsync(Guid tenantId, GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default)
    {
        tenantId = tenantId != default ? tenantId : (await _context.Tenants.FirstAsync(cancellationToken)).Id;

        return await _context.TenantRoles
            .Where(tr => tr.TenantId == tenantId)
            .Select(tr => new RoleDto
            {
                Id = tr.RoleId,
                Name = tr.Role.Name,
                AssignedUsersCount = tr.TenantUserRoles.Count
            })
            .AsNoTracking()
            .GridifyQueryableAsync(gridifyQuery, null, cancellationToken);
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

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
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;

    public RoleService(ITaxBeaconDbContext context, ILogger<RoleService> logger, ICurrentUserService currentUserService, IDateTimeService dateTimeService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
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

    public async Task<OneOf<Success, NotFound>> UnassignUsers(List<Guid> users, Guid tenantId, CancellationToken cancellationToken, Guid roleId)
    {
        var tenant = await _context.Tenants.FirstAsync(cancellationToken);

        var currentUserId = _currentUserService.UserId;
        var role = await _context.TenantRoles
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.RoleId == roleId && x.TenantId == tenant.Id, cancellationToken);
        if (role is null)
        {
            return new NotFound();
        }

        var currentUser = await _context.TenantUsers
            .Include(x => x.User)
            .FirstAsync(x => x.UserId == currentUserId && x.TenantId == tenantId, cancellationToken);
        var usersToRemove = _context.TenantUserRoles
            .Where(x => x.TenantId == tenant.Id && x.RoleId == roleId && users.Contains(x.UserId));

        _context.TenantUserRoles.RemoveRange(usersToRemove);

        var activityUserLogs = new List<UserActivityLog>();
        foreach (var userId in users)
        {
            activityUserLogs.Add(new UserActivityLog
            {
                TenantId = tenant.Id,
                UserId = userId,
                Date = _dateTimeService.UtcNow,
                Revision = 1,
                Event = JsonSerializer.Serialize(
                    new UnassignUsersEvent(
                        role.Role.Name,
                        currentUserId,
                        currentUser.User.FullName,
                        _dateTimeService.UtcNow
                    )),
                EventType = EventType.UserRolesUnassign
            });
        }
        await _context.UserActivityLogs
            .AddRangeAsync(activityUserLogs, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("{dateTime} - Users({userIds}) were unassigned from role({roleId}) by {userId}",
            _dateTimeService.UtcNow,
            string.Join(",", users),
            roleId,
            _currentUserService.UserId);

        return new Success();
    }
}

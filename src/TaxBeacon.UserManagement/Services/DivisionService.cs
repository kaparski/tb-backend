using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.Collections.Immutable;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models.Activities.DivisionsActivities;
using TaxBeacon.UserManagement.Services.Activities.DivisionActivityHistory;
using TaxBeacon.UserManagement.Services.Interfaces;

namespace TaxBeacon.UserManagement.Services;

public class DivisionService: IDivisionSerivce
{
    private readonly ILogger<DivisionService> _logger;
    private readonly ITaxBeaconDbContext _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly IImmutableDictionary<(DivisionEventType, uint), IDivisionActivityFactory> _divisionActivityFactories;

    public DivisionService(
        ILogger<DivisionService> logger,
        ITaxBeaconDbContext context,
        ICurrentUserService currentUserService,
        IEnumerable<IDivisionActivityFactory> divisionActivityFactories)
    {
        _logger = logger;
        _context = context;
        _currentUserService = currentUserService;
        _divisionActivityFactories = divisionActivityFactories?.ToImmutableDictionary(x => (x.EventType, x.Revision))
                                     ?? ImmutableDictionary<(DivisionEventType, uint), IDivisionActivityFactory>.Empty;
    }

    public async Task<OneOf<DivisionActivityDto, NotFound>> GetActivitiesAsync(Guid userId, uint page = 1,
        uint pageSize = 10, CancellationToken cancellationToken = default)
    {
        page = page == 0 ? 1 : page;
        pageSize = pageSize == 0 ? 10 : pageSize;

        var user = await _context.Users
            .Where(u => u.Id == userId && u.TenantUsers.Any(tu => tu.TenantId == _currentUserService.TenantId))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            return new NotFound();
        }

        var userActivitiesQuery = _context.DivisionActivityLogs
            .Where(ua => ua.DivisionId == userId && ua.TenantId == _currentUserService.TenantId);

        var count = await userActivitiesQuery.CountAsync(cancellationToken: cancellationToken);

        var pageCount = (uint)Math.Ceiling((double)count / pageSize);

        var activities = await userActivitiesQuery
            .OrderByDescending(x => x.Date)
            .Skip((int)((page - 1) * pageSize))
            .Take((int)pageSize)
            .ToListAsync(cancellationToken);

        return new DivisionActivityDto(pageCount,
            activities.Select(x => _divisionActivityFactories[(x.EventType, x.Revision)].Create(x.Event)).ToList());
    }
}

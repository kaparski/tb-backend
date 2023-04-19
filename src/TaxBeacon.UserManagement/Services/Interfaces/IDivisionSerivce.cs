using OneOf;
using OneOf.Types;
using TaxBeacon.UserManagement.Models.Activities.DivisionsActivities;

namespace TaxBeacon.UserManagement.Services.Interfaces;

public interface IDivisionSerivce
{
    public Task<OneOf<DivisionActivityDto, NotFound>> GetActivitiesAsync(Guid userId, uint page = 1,
        uint pageSize = 10, CancellationToken cancellationToken = default);
}

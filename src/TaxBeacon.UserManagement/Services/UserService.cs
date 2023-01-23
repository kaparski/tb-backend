using Gridify;
using Mapster;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public class UserService : IUserService
{
    private readonly ITaxBeaconDbContext _dbContext;

    public UserService(ITaxBeaconDbContext dbContext) => _dbContext = dbContext;

    public QueryablePaging<UserList> GetUsers(GridifyQuery gridifyQuery)
    {
        var users = _dbContext
            .Users
            .ProjectToType<UserList>(null)
            .GridifyQueryable(gridifyQuery);

        return users;
    }
}

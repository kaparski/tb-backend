using Gridify;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IUserService
{
    public QueryablePaging<UserList> GetUsers(GridifyQuery gridifyQuery);
}

using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.API.Controllers.Users.Responses;

public class RoleResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;
}

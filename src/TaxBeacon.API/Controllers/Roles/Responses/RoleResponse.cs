namespace TaxBeacon.API.Controllers.Roles.Responses;

public class RoleResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int AssignedUsersCount { get; set; }
}

namespace TaxBeacon.UserManagement.Models;

public class RoleAssignedUserDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;
}

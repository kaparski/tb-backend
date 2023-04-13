namespace TaxBeacon.UserManagement.Models;

public class TeamDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int NumberOfUsers { get; set; }
}

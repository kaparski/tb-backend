namespace TaxBeacon.UserManagement.Teams.Models;

public sealed record UpdateTeamDto
{
    public string Name { get; init; } = string.Empty;

    public string? Description { get; init; }
}

namespace TaxBeacon.Administration.Teams.Models;

public record TeamDto
{
    public Guid Id { get; init; }

    public string Name { get; init; } = null!;

    public DateTime CreatedDateTimeUtc { get; init; }

    public string? Description { get; init; }

    public int NumberOfUsers { get; init; }
}

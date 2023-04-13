namespace TaxBeacon.API.Controllers.Teams.Responses;

public class TeamResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int NumberOfUsers { get; set; }
}

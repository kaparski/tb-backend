namespace TaxBeacon.API.Controllers.Teams.Responses;

public class TeamDetailsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? LastModifiedDateTimeUtc { get; set; }
}

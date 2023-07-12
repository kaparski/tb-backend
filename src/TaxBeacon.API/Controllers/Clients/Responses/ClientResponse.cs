using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Clients.Responses;

public record ClientResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? City { get; set; }

    public State? State { get; set; }

    public string? PrimaryContactName { get; set; }

    public Status Status { get; set; }
}

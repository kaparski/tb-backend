using System.ComponentModel.DataAnnotations;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Accounts.Responses;

public class ClientProspectResponse
{
    [Key]
    public Guid AccountId { get; set; }

    public string Name { get; set; } = null!;

    public string? City { get; set; }

    public State? State { get; set; }

    public Status Status { get; set; }

    public int DaysOpen { get; set; }
}

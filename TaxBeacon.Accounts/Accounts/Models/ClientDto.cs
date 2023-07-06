using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public class ClientDto
{
    public string Name { get; set; }

    public string City { get; set; }

    public string State { get; set; }

    public Status Status { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime ActivationDateTimeUtc { get; set; }
}

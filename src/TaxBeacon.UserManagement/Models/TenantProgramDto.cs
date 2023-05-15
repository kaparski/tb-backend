using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Models;

public class TenantProgramDto
{
    public Guid Id { get; set; }

    public string Reference { get; set; }

    public string Name { get; set; }

    public Jurisdiction Jurisdiction { get; set; }

    public string JurisdictionName { get; set; }

    public string IncentivesArea { get; set; }

    public string IncentivesType { get; set; }

    public Status Status { get; set; }

    public DateTime StartDateTimeUtc { get; set; }

    public DateTime EndDateTimeUtc { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }
}

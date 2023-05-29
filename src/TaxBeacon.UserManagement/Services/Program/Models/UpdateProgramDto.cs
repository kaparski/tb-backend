using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Services.Program.Models;

public class UpdateProgramDto
{
    public string Name { get; set; } = null!;

    public string? Reference { get; set; }

    public string? Overview { get; set; }

    public string? LegalAuthority { get; set; }

    public string? Agency { get; set; }

    public Jurisdiction Jurisdiction { get; set; }

    public string? State { get; set; }

    public string? County { get; set; }

    public string? City { get; set; }

    public string? IncentivesArea { get; set; }

    public string? IncentivesType { get; set; }

    public DateTime? StartDateTimeUtc { get; set; }

    public DateTime? EndDateTimeUtc { get; set; }
}

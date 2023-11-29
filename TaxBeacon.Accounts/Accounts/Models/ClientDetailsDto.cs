using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.Accounts.Models;

public record ClientDetailsDto
{
    public string State { get; init; } = null!;
    public Status? Status { get; init; }
    public decimal? AnnualRevenue { get; init; }
    public int? FoundationYear { get; init; }
    public int? EmployeeCount { get; init; }
    public DateTime? DeactivationDateTimeUtc { get; init; }
    public DateTime? ReactivationDateTimeUtc { get; init; }
    public DateTime CreatedDateTimeUtc { get; init; }
    public DateTime? LastModifiedDateTimeUtc { get; init; }
    public ContactDto? PrimaryContact { get; init; } = null!;
    public ICollection<ClientManagerDto> ClientManagers { get; init; } = new List<ClientManagerDto>();
}

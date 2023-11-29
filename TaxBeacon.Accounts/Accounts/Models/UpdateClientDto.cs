namespace TaxBeacon.Accounts.Accounts.Models;
public record UpdateClientDto()
{
    public decimal? AnnualRevenue { get; init; }
    public int? FoundationYear { get; init; }
    public int? EmployeeCount { get; init; }
    public Guid? PrimaryContactId { get; init; }
    public IEnumerable<Guid> ClientManagersIds { get; init; } = Enumerable.Empty<Guid>();

    public virtual bool Equals(UpdateClientDto? obj) => obj is not null
            && AnnualRevenue == obj.AnnualRevenue
            && FoundationYear == obj.FoundationYear
            && EmployeeCount == obj.EmployeeCount
            && PrimaryContactId == obj.PrimaryContactId;

    public override int GetHashCode() => base.GetHashCode();
}

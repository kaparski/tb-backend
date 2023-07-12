namespace TaxBeacon.Common.Options;

public sealed class KnownAadTenant
{
    public string DomainName { get; set; } = null!;
    public string IssuerUrl { get; set; } = null!;
}

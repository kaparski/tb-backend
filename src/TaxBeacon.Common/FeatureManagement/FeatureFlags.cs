namespace TaxBeacon.Common.FeatureManagement;

public sealed record FeatureFlags
{
    public IDictionary<string, bool> Flags { get; init; } = new Dictionary<string, bool>();
}

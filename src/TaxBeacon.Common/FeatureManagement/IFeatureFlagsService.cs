namespace TaxBeacon.Common.FeatureManagement;

public interface IFeatureFlagsService
{
    IDictionary<string, bool> GetCurrentFeatureFlags();

    IDictionary<string, bool> UpdateFeatureFlag(IDictionary<string, bool> newFeatureFlags);

    bool IsEnabled(string key);

    IDictionary<string, bool> ResetFeatureFlags();
}

using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.FeatureManagement;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFeatureManagement(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        serviceCollection.AddSingleton<FeatureFlags>(new FeatureFlags
        {
            Flags = configuration
                .GetSection(nameof(FeatureFlags))
                .GetChildren()
                .ToDictionary(s => s.Key, s => s.Get<bool>())
        });
        serviceCollection.AddScoped<IFeatureFlagsService, FeatureFlagsService>();

        return serviceCollection;
    }
}

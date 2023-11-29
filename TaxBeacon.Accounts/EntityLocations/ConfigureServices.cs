using Microsoft.Extensions.DependencyInjection;

namespace TaxBeacon.Accounts.EntityLocations;
public static class ConfigureServices
{
    public static IServiceCollection AddEntityLocations(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IEntityLocationService, EntityLocationsService>();

        return serviceCollection;
    }
}

using Microsoft.Extensions.DependencyInjection;
using TaxBeacon.Accounts.Locations.Activities.Factories;

namespace TaxBeacon.Accounts.Locations;

public static class ConfigureServices
{
    public static IServiceCollection AddLocations(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<ILocationService, LocationService>();
        serviceCollection.AddScoped<ILocationActivityFactory, LocationCreatedEventFactory>();
        serviceCollection.AddScoped<ILocationActivityFactory, LocationReactivatedEventFactory>();
        serviceCollection.AddScoped<ILocationActivityFactory, LocationDeactivatedEventFactory>();
        serviceCollection.AddScoped<ILocationActivityFactory, LocationUpdatedEventFactory>();
        serviceCollection.AddScoped<ILocationActivityFactory, LocationUnassociatedWithEntityEventFactory>();
        serviceCollection.AddScoped<ILocationActivityFactory, LocationAssociatedWithEntityEventFactory>();

        return serviceCollection;
    }
}

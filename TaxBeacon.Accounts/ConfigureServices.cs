using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.Accounts.Contacts;
using TaxBeacon.Accounts.Entities;
using TaxBeacon.Accounts.Locations;

namespace TaxBeacon.Accounts;

public static class ConfigureServices
{
    public static IServiceCollection AddAccountsServices(this IServiceCollection serviceCollection)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        serviceCollection.AddAccounts();
        serviceCollection.AddScoped<IContactService, ContactService>();
        serviceCollection.AddScoped<ILocationService, LocationService>();
        serviceCollection.AddScoped<IEntityService, EntityService>();

        return serviceCollection;
    }
}

using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.Accounts.Services.Contacts;
using TaxBeacon.Accounts.Accounts;
using Microsoft.Extensions.DependencyInjection;
using TaxBeacon.Accounts.Locations;
using TaxBeacon.Accounts.Services.Entities;

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

using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.Accounts.Services.Contacts;
using TaxBeacon.Accounts.Accounts;
using Microsoft.Extensions.DependencyInjection;
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

        return serviceCollection;
    }
}

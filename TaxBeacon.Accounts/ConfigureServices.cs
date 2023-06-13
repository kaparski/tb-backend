using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.Accounts.Accounts;

namespace TaxBeacon.Accounts;

public static class ConfigureServices
{
    public static IServiceCollection AddAccountsServices(this IServiceCollection serviceCollection)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        serviceCollection.AddAccounts();
        
        return serviceCollection;
    }
}

using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.Accounts.Services.Contacts;

namespace TaxBeacon.Accounts;

public static class ConfigureServices
{
    public static IServiceCollection AddAccountManagementServices(this IServiceCollection serviceCollection)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        serviceCollection.AddScoped<IContactService, ContactService>();

        return serviceCollection;
    }
}

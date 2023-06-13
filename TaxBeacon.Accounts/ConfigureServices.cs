using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.Accounts.Services.Contacts;
using TaxBeacon.Accounts.Entities;

namespace TaxBeacon.Accounts;

public static class ConfigureServices
{
    public static IServiceCollection AddAccountManagementServices(this IServiceCollection serviceCollection)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        serviceCollection.AddScoped<IContactService, ContactService>();
        serviceCollection.AddScoped<IEntityService, EntityService>();

        return serviceCollection;
    }
}

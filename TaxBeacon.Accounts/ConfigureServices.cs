using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.Accounts.Common.Services;
using TaxBeacon.Accounts.Contacts;
using TaxBeacon.Accounts.Documents;
using TaxBeacon.Accounts.Entities;
using TaxBeacon.Accounts.EntityLocations;
using TaxBeacon.Accounts.Locations;
using TaxBeacon.Accounts.Naics;

namespace TaxBeacon.Accounts;

public static class ConfigureServices
{
    public static IServiceCollection AddAccountsServices(this IServiceCollection serviceCollection)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        serviceCollection.AddAccounts();
        serviceCollection.AddEntities();
        serviceCollection.AddLocations();
        serviceCollection.AddDocuments();
        serviceCollection.AddEntityLocations();
        serviceCollection.AddContacts();
        serviceCollection.AddScoped<INaicsService, NaicsService>();
        serviceCollection.AddSingleton<ICsvService, CsvService>();

        return serviceCollection;
    }
}

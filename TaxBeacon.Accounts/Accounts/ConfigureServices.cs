using Microsoft.Extensions.DependencyInjection;
using TaxBeacon.Accounts.Accounts.Activities.Factories;

namespace TaxBeacon.Accounts.Accounts;

public static class ConfigureServices
{
    public static IServiceCollection AddAccounts(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAccountService, AccountService>();
        serviceCollection.AddScoped<IAccountActivityFactory, ClientDeactivatedEventFactory>();
        serviceCollection.AddScoped<IAccountActivityFactory, ClientReactivatedEventFactory>();

        return serviceCollection;
    }
}

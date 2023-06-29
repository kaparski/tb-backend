using Microsoft.Extensions.DependencyInjection;

namespace TaxBeacon.Accounts.Accounts;

public static class ConfigureServices
{
    public static IServiceCollection AddAccounts(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAccountService, AccountService>();

        return serviceCollection;
    }
}

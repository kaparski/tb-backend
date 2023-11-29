using Microsoft.Extensions.DependencyInjection;
using TaxBeacon.Accounts.Accounts.Activities.Factories;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Accounts;

public static class ConfigureServices
{
    public static IServiceCollection AddAccounts(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IAccountService, AccountService>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, ClientDeactivatedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, ClientReactivatedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, AccountProfileUpdatedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, ClientAccountCreatedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, ClientUpdatedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, ClientAccountManagerAssignedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, ClientAccountManagerUnassignedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, SalespersonAssignedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, SalespersonUnassignedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, EntitiesImportedSuccessfullyEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, EntitiesImportFailedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, ReferralAccountCreatedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<AccountEventType>, ReferralAccountManagerAssignedEventFactory>();

        return serviceCollection;
    }
}

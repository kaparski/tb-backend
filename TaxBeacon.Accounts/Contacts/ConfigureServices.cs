using Microsoft.Extensions.DependencyInjection;
using TaxBeacon.Accounts.Contacts.Activities.Factories;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.Contacts;

public static class ConfigureServices
{
    public static IServiceCollection AddContacts(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IContactService, ContactService>();
        serviceCollection.AddScoped<IActivityFactory<ContactEventType>, ContactCreatedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<ContactEventType>, ContactAssignedToAccountEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<ContactEventType>, ContactDeactivatedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<ContactEventType>, ContactReactivatedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<ContactEventType>, ContactUnassociatedWithAccountEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<ContactEventType>, ContactUnlinkedFromContactEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<ContactEventType>, ContactUpdatedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<ContactEventType>, ContactTypeUpdatedEventFactory>();
        serviceCollection.AddScoped<IActivityFactory<ContactEventType>, ContactLinkedToContactEventFactory>();

        return serviceCollection;
    }
}

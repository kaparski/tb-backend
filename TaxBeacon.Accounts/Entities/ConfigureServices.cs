using Microsoft.Extensions.DependencyInjection;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Exporters;

namespace TaxBeacon.Accounts.Entities;

public static class ConfigureServices
{
    public static IServiceCollection AddEntities(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IEntityService, EntityService>();
        serviceCollection.AddScoped<IEntityActivityFactory, EntityCreatedEventFactory>();
        serviceCollection.AddScoped<IEntityActivityFactory, EntityUpdatedEventFactory>();
        serviceCollection.AddScoped<IEntityActivityFactory, EntityDeactivatedEventFactory>();
        serviceCollection.AddScoped<IEntityActivityFactory, EntityReactivatedEventFactory>();
        serviceCollection.AddScoped<IEntityActivityFactory, StateIdAddedEventFactory>();
        serviceCollection.AddScoped<IEntityActivityFactory, StateIdDeletedEventFactory>();
        serviceCollection.AddScoped<IEntityActivityFactory, StateIdUpdatedEventFactory>();
        serviceCollection.AddScoped<IEntityActivityFactory, EntityUnassociatedWithLocationEventFactory>();
        serviceCollection.AddScoped<IEntityActivityFactory, EntityAssociatedWithLocationEventFactory>();
        serviceCollection.AddScoped<IEntityActivityFactory, EntityImportedEventFactory>();
        serviceCollection.AddScoped<IAccountEntitiesToCsvExporter, AccountEntitiesToCsvExporter>();
        serviceCollection.AddScoped<IAccountEntitiesToXlsxExporter, AccountEntitiesToXlsxExporter>();

        return serviceCollection;
    }
}

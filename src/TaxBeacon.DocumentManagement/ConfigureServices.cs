using Microsoft.Extensions.DependencyInjection;
using TaxBeacon.DocumentManagement.BlobStorage;
using TaxBeacon.DocumentManagement.Templates;

namespace TaxBeacon.DocumentManagement;

public static class ConfigureServices
{
    public static IServiceCollection AddDocumentManagementServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IBlobStorageService, BlobStorageService>();
        serviceCollection.AddScoped<ITemplatesService, TemplatesService>();

        return serviceCollection;
    }
}

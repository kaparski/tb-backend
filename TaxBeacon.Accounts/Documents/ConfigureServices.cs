using Microsoft.Extensions.DependencyInjection;

namespace TaxBeacon.Accounts.Documents;
public static class ConfigureServices
{
    public static IServiceCollection AddDocuments(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDocumentService, DocumentService>();

        return serviceCollection;
    }
}

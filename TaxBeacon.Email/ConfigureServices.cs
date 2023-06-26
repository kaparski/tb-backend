using Microsoft.Extensions.DependencyInjection;

namespace TaxBeacon.Email;
public static class ConfigureServices
{
    public static IServiceCollection AddEmailServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IEmailSender, SendGridEmailSender>();
        return serviceCollection;
    }
}

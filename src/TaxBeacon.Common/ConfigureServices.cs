using Microsoft.Extensions.DependencyInjection;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Common;

public static class ConfigureServices
{
    public static IServiceCollection AddCommonServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IDateTimeService, DateTimeService>();
        serviceCollection.AddScoped<IListToFileConverter, ListToCsvConverter>();
        serviceCollection.AddScoped<IListToFileConverter, ListToXlsxConverter>();

        return serviceCollection;
    }
}

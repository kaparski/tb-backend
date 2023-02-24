using Gridify;

namespace TaxBeacon.API.Extensions.GridifyServices;

public static class GridifyServiceExtension
{
    internal static void AddGridify(this IServiceCollection services, IConfiguration config)
    {
        services.ConfigureOptions<GridifyOptionsSetup>();
        var settings = config
            .GetSection(GridifyOptionsSetup.ConfigurationSectionName)
            .Get<GridifyOptions>();

        GridifyGlobalConfiguration.CaseSensitiveMapper = false;
        if (settings != null)
        {
            GridifyGlobalConfiguration.DefaultPageSize = settings.DefaultPageSize;
            GridifyGlobalConfiguration.AllowNullSearch = settings.AllowNullSearch;
        }
    }
}

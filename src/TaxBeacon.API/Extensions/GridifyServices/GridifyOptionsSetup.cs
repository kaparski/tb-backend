using Microsoft.Extensions.Options;

namespace TaxBeacon.API.Extensions.GridifyServices;

public class GridifyOptionsSetup: IConfigureOptions<GridifyOptions>
{
    public const string ConfigurationSectionName = "GridifyOptions";
    private readonly IConfiguration _configuration;

    public GridifyOptionsSetup(IConfiguration configuration) => _configuration = configuration;

    public void Configure(GridifyOptions options) =>
        _configuration
            .GetSection(ConfigurationSectionName)
            .Bind(options);
}

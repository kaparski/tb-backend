namespace TaxBeacon.API.Extensions;

public static class EnvironmentExtensions
{
    public static readonly string CloudDevelopment = "CloudDevelopment";

    public static bool IsCloudDevelopment(this IHostEnvironment hostEnvironment) => hostEnvironment.IsEnvironment(CloudDevelopment);
}

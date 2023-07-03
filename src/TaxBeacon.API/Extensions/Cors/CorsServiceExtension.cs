namespace TaxBeacon.API.Extensions.Cors;

public static class CorsServiceExtension
{
    public static void AddCorsService(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration
            .GetSection("Cors")
            .Get<CorsOptions>();

        services.AddCors(options => options.AddPolicy("DefaultCorsPolicy", policy => policy
                    .WithOrigins(settings?.AllowedOrigins ?? Array.Empty<string>())
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders("Content-Disposition")));
    }
}

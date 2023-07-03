namespace TaxBeacon.API.Extensions.Cors;

public class CorsOptions
{
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
}

namespace TaxBeacon.API.Extensions.Swagger;

public class SwaggerAzureAdOptions
{
    public string TokenUrl { get; set; } = string.Empty;

    public string AuthorizationUrl { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string[] ApiScopes { get; set; } = { };
}

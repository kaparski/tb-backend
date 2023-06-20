namespace TaxBeacon.Common.Options;

public sealed class AzureAd
{
    public string Domain { get; set; } = string.Empty;

    public string TenantId { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string Secret { get; set; } = string.Empty;

}

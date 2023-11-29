using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

namespace TaxBeacon.API.Extensions.KeyVault;

public static class KeyVaultExtensions
{
    public static ConfigurationManager AddKeyVault(this ConfigurationManager configuration)
    {
        var keyVaultUrl = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_RESOURCEENDPOINT");

        if (string.IsNullOrEmpty(keyVaultUrl))
        {
            return configuration;
        }

        var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        configuration.AddAzureKeyVault(client, new AzureKeyVaultConfigurationOptions());

        return configuration;
    }
}

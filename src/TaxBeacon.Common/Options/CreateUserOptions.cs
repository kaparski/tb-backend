namespace TaxBeacon.Common.Options;

public sealed class CreateUserOptions
{
    public const string CreateUser = "CreateUser";

    public string[] Recipients { get; set; } = Array.Empty<string>();

    public string[] RegisteredDomains { get; set; } = Array.Empty<string>();

    public KnownAadTenant[] KnownAadTenants { get; set; } = Array.Empty<KnownAadTenant>();
}

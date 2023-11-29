namespace TaxBeacon.API.Authentication;

public class LoadTestingOptions
{
    public const string LoadTesting = "LoadTesting";

    public bool IsLoadTestingEnabled { get; set; } = false;

    public ICollection<LoadTestingUser> LoadTestingUsers { get; set; } = new HashSet<LoadTestingUser>();

    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public double TokenLifeTime { get; set; } = 60;
}

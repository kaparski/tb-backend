namespace TaxBeacon.Email.Options;
public class SendGridOptions
{
    public const string SendGrid = "SendGrid";

    public string ApiKey { get; set; } = null!;

    public string From { get; set; } = null!;
}

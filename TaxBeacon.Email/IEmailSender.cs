namespace TaxBeacon.Email;

public interface IEmailSender
{
    Task SendAsync<T>(EmailType emailType, string[] recipients, T message) where T : IEmailMessage;
}

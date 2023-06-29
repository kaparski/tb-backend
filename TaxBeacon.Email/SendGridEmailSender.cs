using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SendGrid;
using SendGrid.Helpers.Mail;
using TaxBeacon.Email.Messages;
using TaxBeacon.Email.Options;

namespace TaxBeacon.Email;
public class SendGridEmailSender: IEmailSender
{
    private readonly SendGridOptions _sendGridOptions;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(IOptionsSnapshot<SendGridOptions> optionsSnapshot, ILogger<SendGridEmailSender> logger)
    {
        _sendGridOptions = optionsSnapshot.Value;
        _logger = logger;
    }

    public async Task SendAsync<T>(EmailType emailType, string[] recipients, T message) where T : IEmailMessage
    {
        if (string.IsNullOrEmpty(_sendGridOptions.ApiKey))
        {
            throw new InvalidOperationException("api key must be provided");
        }

        if (string.IsNullOrEmpty(_sendGridOptions.From))
        {
            throw new InvalidOperationException("sender email must be provided");
        }

        _logger.LogInformation("Attempt to send email message of type {messageType} to {recipients}", typeof(T).Name, string.Join(" ", recipients));

        var client = new SendGridClient(_sendGridOptions.ApiKey);

        if (emailType == EmailType.UserCreated && message is UserCreatedMessage userCreatedMessage && recipients.Any())
        {
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_sendGridOptions.From, "Tax Beacon System"),
                Subject = "Tax Beacon account details",
                PlainTextContent =
                $"""
                You have been registered in Tax Beacon system with the following credentials:

                Username: {userCreatedMessage.Email}

                Password: {userCreatedMessage.Password}
                """
            };

            msg.AddTos(recipients.Select(r => new EmailAddress(r)).ToList());

            var response = await client.SendEmailAsync(msg);

            _logger.LogInformation("User {userEmail} creation information has been sent, response status code is {StatusCode}", userCreatedMessage.Email, response.StatusCode);
        }
    }
}

namespace TaxBeacon.Email.Messages;
public sealed class UserCreatedMessage: IEmailMessage
{
    public string Email { get; }

    public string Password { get; }

    public UserCreatedMessage(string email, string password)
    {
        Email = email;
        Password = password;
    }
}

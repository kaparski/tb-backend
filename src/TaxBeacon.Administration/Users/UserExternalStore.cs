using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System.Net.Mail;
using TaxBeacon.Administration.PasswordGenerator;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Options;

namespace TaxBeacon.Administration.Users;

public sealed class UserExternalStore: IUserExternalStore
{
    private readonly IPasswordGenerator _passwordGenerator;
    private readonly AzureAd _azureAd;

    public UserExternalStore(IPasswordGenerator passwordGenerator, IOptions<AzureAd> azureAdOptions)
    {
        _passwordGenerator = passwordGenerator;
        _azureAd = azureAdOptions.Value;
    }

    public async Task<(string aadB2CObjectId, UserType userType, string password)> CreateUserAsync(MailAddress mailAddress,
        string firstName,
        string lastName,
        string? externalAadTenantIssuerUrl,
        string? externalAadUserObjectId,
        CancellationToken cancellationToken)
    {
        var credentials = new ClientSecretCredential(_azureAd.TenantId, _azureAd.ClientId, _azureAd.Secret);
        var graphClient = new GraphServiceClient(credentials);

        var existingUsers = await graphClient
            .Users
            .Request()
            .Filter($"(mail eq '{mailAddress.Address}') or (otherMails/any(m:m eq '{mailAddress.Address}'))")
            .GetAsync(cancellationToken);

        if (existingUsers.Count > 1)
        {
            throw new InvalidOperationException("Multiple users with such an email found");
        }

        if (existingUsers.Count == 1)
        {
            // Just returning this user's ID in our B2C tenant
            var existingUser = existingUsers[0];
            return (existingUser.Id, UserType.ExistingB2C, string.Empty);
        }

        User user;
        var password = string.Empty;
        UserType userType;

        if (string.IsNullOrEmpty(externalAadTenantIssuerUrl))
        {
            // Creating a B2C user
            userType = UserType.LocalB2C;

            password = _passwordGenerator.GeneratePassword();

            user = new User
            {
                Mail = mailAddress.Address,
                DisplayName = $"{firstName} {lastName}",
                Identities = new List<ObjectIdentity>()
                {
                    new ObjectIdentity
                    {
                        SignInType = "emailAddress",
                        Issuer = _azureAd.Domain,
                        IssuerAssignedId = mailAddress.Address
                    },
                },
                PasswordProfile = new PasswordProfile
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = false
                },
                PasswordPolicies = "DisablePasswordExpiration, DisableStrongPassword"
            };
        }
        else
        {
            // Creating an external AAD tenant user
            userType = UserType.ExternalAad;

            user = new User
            {
                Mail = mailAddress.Address,
                DisplayName = $"{firstName} {lastName}",
                Identities = new List<ObjectIdentity>()
                {
                    new ObjectIdentity
                    {
                        SignInType = "federated",
                        Issuer = externalAadTenantIssuerUrl,
                        IssuerAssignedId = externalAadUserObjectId
                    },
                }
            };
        }

        var createdUser = await graphClient.Users
            .Request()
            .AddAsync(user, cancellationToken);

        // Returning user's ID in our B2C tenant. And a password, if it is a newly created B2C user.
        return (createdUser.Id, userType, password);
    }
}

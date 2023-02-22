﻿using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using System.Net.Mail;
using TaxBeacon.Common.Options;

namespace TaxBeacon.UserManagement.Services
{
    public sealed class UserExternalStore: IUserExternalStore
    {
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly AzureAD _azureAd;

        public UserExternalStore(IPasswordGenerator passwordGenerator, IOptions<AzureAD> _azureAdOptions)
        {
            _passwordGenerator = passwordGenerator;
            _azureAd = _azureAdOptions.Value;
        }

        public async Task<string> CreateUserAsync(MailAddress mailAddress, string firstName, string lastName)
        {
            var credentials = new ClientSecretCredential(_azureAd.TenantId, _azureAd.ClientId, _azureAd.Secret);
            var graphClient = new GraphServiceClient(credentials);

            var existedUser = await graphClient.Users.Request().Filter($"mail eq '{mailAddress.Address}'").GetAsync();
            if (existedUser.Count != 0)
            {
                return string.Empty;
            }

            var user = new User
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
                    }
                },
                PasswordProfile = new PasswordProfile
                {
                    Password = _passwordGenerator.GeneratePassword(),
                    ForceChangePasswordNextSignIn = false
                },
                PasswordPolicies = "DisablePasswordExpiration, DisableStrongPassword"
            };

            await graphClient.Users
                .Request()
                .AddAsync(user);

            return user.PasswordProfile.Password;
        }
    }
}

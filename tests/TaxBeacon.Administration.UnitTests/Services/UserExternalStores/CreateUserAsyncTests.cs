using FluentAssertions.Execution;
using Microsoft.Graph;
using Moq;
using System.Net.Mail;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.UserExternalStores;
public partial class UserExternalStoreTests
{
    [Fact]
    public async Task CreateUserAsync_LocalUser_CreatedSuccessfully()
    {
        //Arrange

        var user = TestData.TestUser.Generate();

        _requestMock
            .Setup(m => m.Filter($"(mail eq '{user.Email}') or (otherMails/any(m:m eq '{user.Email}'))"))
            .Returns(_requestMock.Object);

        _requestMock
            .Setup(m => m.GetAsync(default))
            .Returns(Task.FromResult(new GraphServiceUsersCollectionPage() as IGraphServiceUsersCollectionPage));

        var createdUser = new User { Id = Guid.NewGuid().ToString() };

        Func<User, bool> userValidator = u =>
        {
            u.Mail.Should().Be(user.Email);
            u.DisplayName.Should().Be($"{user.FirstName} {user.LastName}");
            u.Identities.Single().SignInType.Should().Be("emailAddress");
            u.Identities.Single().Issuer.Should().Be(_optionsMock.Object.Value.Domain);
            u.PasswordProfile.Password.Should().Be(user.Password);
            u.PasswordPolicies.Should().Be("DisablePasswordExpiration, DisableStrongPassword");

            return true;
        };

        _requestMock
            .Setup(m => m.AddAsync(It.Is<User>(u => userValidator(u)), default))
            .Returns(Task.FromResult(createdUser));

        _passwordGeneratorMock
            .Setup(m => m.GeneratePassword())
            .Returns(user.Password);

        //Act

        var (objectId, userType, password) = await _store.CreateUserAsync(new MailAddress(user.Email), user.FirstName, user.LastName, default, default, default);

        //Assert

        using (new AssertionScope())
        {
            objectId.Should().Be(createdUser.Id);
            userType.Should().Be(Common.Enums.UserType.LocalB2C);
            password.Should().Be(user.Password);
        }
    }

    [Fact]
    public async Task CreateUserAsync_AadUser_CreatedSuccessfully()
    {
        //Arrange

        var user = TestData.TestUser.Generate();

        _requestMock
            .Setup(m => m.Filter($"(mail eq '{user.Email}') or (otherMails/any(m:m eq '{user.Email}'))"))
            .Returns(_requestMock.Object);

        _requestMock
            .Setup(m => m.GetAsync(default))
            .Returns(Task.FromResult(new GraphServiceUsersCollectionPage() as IGraphServiceUsersCollectionPage));

        var createdUser = new User { Id = Guid.NewGuid().ToString() };

        Func<User, bool> userValidator = u =>
        {
            u.Mail.Should().Be(user.Email);
            u.DisplayName.Should().Be($"{user.FirstName} {user.LastName}");
            u.Identities.Single().SignInType.Should().Be("federated");
            u.Identities.Single().Issuer.Should().Be(user.ExternalAadTenantIssuerUrl);
            u.Identities.Single().IssuerAssignedId.Should().Be(user.ExternalAadUserObjectId);

            return true;
        };

        _requestMock
            .Setup(m => m.AddAsync(It.Is<User>(u => userValidator(u)), default))
            .Returns(Task.FromResult(createdUser));

        //Act

        var (objectId, userType, password) = await _store.CreateUserAsync(new MailAddress(user.Email),
            user.FirstName,
            user.LastName,
            user.ExternalAadTenantIssuerUrl,
            user.ExternalAadUserObjectId,
            default);

        //Assert

        using (new AssertionScope())
        {
            objectId.Should().Be(createdUser.Id);
            userType.Should().Be(Common.Enums.UserType.ExternalAad);
            password.Should().BeEmpty();
        }
    }

    [Fact]
    public async Task CreateUserAsync_UserAlreadyExists_ExistingUserIdReturned()
    {
        //Arrange

        var user = TestData.TestUser.Generate();

        _requestMock
            .Setup(m => m.Filter($"(mail eq '{user.Email}') or (otherMails/any(m:m eq '{user.Email}'))"))
            .Returns(_requestMock.Object);

        var existingUser = new User { Id = Guid.NewGuid().ToString() };

        _requestMock
            .Setup(m => m.GetAsync(default))
            .Returns(Task.FromResult(new GraphServiceUsersCollectionPage { existingUser } as IGraphServiceUsersCollectionPage));

        //Act

        var (objectId, userType, password) = await _store.CreateUserAsync(new MailAddress(user.Email),
            user.FirstName,
            user.LastName,
            default,
            default,
            default);

        //Assert

        using (new AssertionScope())
        {
            objectId.Should().Be(existingUser.Id);
            userType.Should().Be(Common.Enums.UserType.ExistingB2C);
            password.Should().BeEmpty();
        }
    }
}

using Azure.Identity;
using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Moq;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;
using System.Net.Mail;
using TaxBeacon.Administration.PasswordGenerator;
using TaxBeacon.Administration.Users;
using TaxBeacon.Common.Options;

namespace TaxBeacon.Administration.UnitTests.Services;

public class UserExternalStoreTests
{
    private readonly Mock<GraphServiceClient> _graphClientMock;
    private readonly Mock<IGraphServiceUsersCollectionRequestBuilder> _requestBuilderMock = new();
    private readonly Mock<IGraphServiceUsersCollectionRequest> _requestMock = new();
    private readonly Mock<IPasswordGenerator> _passwordGeneratorMock = new();
    private readonly Mock<IOptions<AzureAd>> _optionsMock = new();
    private readonly UserExternalStore _store;

    public UserExternalStoreTests()
    {
        _graphClientMock = new Mock<GraphServiceClient>(new Mock<IAuthenticationProvider>().Object, new Mock<IHttpProvider>().Object);

        _requestBuilderMock.Setup(m => m.Request()).Returns(_requestMock.Object);
        _graphClientMock.SetupGet(m => m.Users).Returns(_requestBuilderMock.Object);

        _optionsMock
            .SetupGet(m => m.Value)
            .Returns(TestData.TestAzureAdConfig.Generate());

        _store = new UserExternalStore(_passwordGeneratorMock.Object, _graphClientMock.Object, _optionsMock.Object);
    }

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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public class TestUserDto
        {
            public string FirstName { get; set; } = null!;
            public string LastName { get; set; } = null!;
            public string Email { get; set; } = null!;
            public string Password { get; set; } = null!;
            public string ExternalAadTenantIssuerUrl { get; set; } = null!;
            public string ExternalAadUserObjectId { get; set; } = null!;
        }

        public static readonly Faker<TestUserDto> TestUser =
            new Faker<TestUserDto>()
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.Password, f => f.Hacker.Noun())
                .RuleFor(u => u.ExternalAadTenantIssuerUrl, f => f.Internet.Url())
                .RuleFor(u => u.ExternalAadUserObjectId, Guid.NewGuid().ToString())
        ;

        public static readonly Faker<AzureAd> TestAzureAdConfig =
            new Faker<AzureAd>()
                .RuleFor(d => d.Domain, f => f.Company.CompanyName())
        ;

    }
}

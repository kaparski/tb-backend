using Bogus;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Moq;
using TaxBeacon.Administration.PasswordGenerator;
using TaxBeacon.Administration.Users;
using TaxBeacon.Common.Options;

namespace TaxBeacon.Administration.UnitTests.Services.UserExternalStores;
public partial class UserExternalStoreTests
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
                .RuleFor(u => u.ExternalAadUserObjectId, Guid.NewGuid().ToString());

        public static readonly Faker<AzureAd> TestAzureAdConfig =
            new Faker<AzureAd>()
                .RuleFor(d => d.Domain, f => f.Company.CompanyName());

    }
}

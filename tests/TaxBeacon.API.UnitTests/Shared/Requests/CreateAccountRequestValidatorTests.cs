using Bogus;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.API.Controllers.Accounts.Requests;
using TaxBeacon.API.Shared.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.Accounts.Requests;
public class AccountWithAccountIdRequestValidationTests
{
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly AccountWithAccountIdRequestValidation _requestValidator;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public AccountWithAccountIdRequestValidationTests()
    {
        _httpContextAccessorMock = new();
        _entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(CreateAccountRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _requestValidator = new AccountWithAccountIdRequestValidation();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("A1231231")]
    public async Task Validation_RequiredFieldAccountId_ShouldCheckAccountIdRequired(string accountId)
    {
        //Arrange
        var createRequest = TestData.CreateAccountRequestFaker
            .RuleFor(r => r.AccountId, _ => accountId)
            .Generate();

        //Act
        var actualResult = await _requestValidator.TestValidateAsync(createRequest, null, default);

        //Assert
        if (string.IsNullOrWhiteSpace(accountId))
            actualResult.ShouldHaveValidationErrorFor(r => r.AccountId);
        else
            actualResult.ShouldNotHaveValidationErrorFor(r => r.AccountId);
    }

    private static class TestData
    {

        public static Faker<Account> AccountsFaker =>
            new Faker<Account>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Country, f => f.Address.Country())
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.State, f => f.PickRandom<State>())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Address1, f => f.Address.StreetAddress())
                .RuleFor(a => a.Address2, f => f.Address.StreetAddress())
                .RuleFor(a => a.Zip, f => f.Random.Number(10000, 9999999).ToString())
                .RuleFor(a => a.County, f => f.Address.County())
                .RuleFor(a => a.LastModifiedDateTimeUtc, f => f.Date.Past(1, DateTime.UtcNow));

        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static Faker<CreateAccountRequest> CreateAccountRequestFaker =>
            new Faker<CreateAccountRequest>()
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Country, f => Country.UnitedStates)
                .RuleFor(a => a.State, f => f.PickRandom<State>())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Address1, f => f.Address.StreetAddress())
                .RuleFor(a => a.Address2, f => f.Address.StreetAddress())
                .RuleFor(a => a.Zip, f => f.Random.Number(10000, 9999999).ToString())
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.County, f => f.Address.County());
    }
}

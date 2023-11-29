using Bogus;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.API.Controllers.Locations.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.Locations;

public class UpdateLocationRequestValidatorTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly UpdateLocationRequestValidator _updateLocationRequestValidator;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<INaicsService> _naicsServiceMock;
    public UpdateLocationRequestValidatorTests()
    {
        _currentUserServiceMock = new();
        _httpContextAccessorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UpdateLocationRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);
        _naicsServiceMock = new();
        _updateLocationRequestValidator = new UpdateLocationRequestValidator(_httpContextAccessorMock.Object, _dbContextMock, _currentUserServiceMock.Object, _naicsServiceMock.Object);
    }

    [Fact]
    public async Task Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("locationId", location.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateLocationRequest = TestData.UpdateLocationRequestFaker
            .Generate();

        //Act
        var actualResult = await _updateLocationRequestValidator.TestValidateAsync(updateLocationRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validation_EmptyName_ShouldHaveErrors(string name)
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        var updateUserRequest = TestData.UpdateLocationRequestFaker
            .RuleFor(r => r.Name, _ => name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("locationId", location.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateLocationRequestValidator.TestValidateAsync(updateUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
    }

    [Fact]
    public async Task Validation_LongName_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.UpdateLocationRequestFaker
            .RuleFor(r => r.Name, f => f.Random.String2(200))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("locationId", location.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("The location name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
    }

    [Fact]
    public async Task Validation_LocationWithNameAlreadyExists_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var existingLocations = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(existingLocations);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.UpdateLocationRequestFaker
            .RuleFor(r => r.Name, _ => existingLocations[0].Name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("locationId", existingLocations[1].Id);
        httpContext.Request.RouteValues.Add("accountId", account.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("Location with the same name already exists");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validation_EmptyLocationId_ShouldHaveErrors(string locationId)
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("locationId", location.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var createUserRequest = TestData.UpdateLocationRequestFaker
            .RuleFor(r => r.LocationId, _ => locationId)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _updateLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
    }

    [Fact]
    public async Task Validation_LongLocationId_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.UpdateLocationRequestFaker
            .RuleFor(r => r.LocationId, f => f.Random.String2(200))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("locationId", location.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);
        //Act
        var actualResult = await _updateLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldHaveValidationErrorFor(r => r.LocationId)
            .WithErrorMessage("The location id must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
    }

    [Fact]
    public async Task Validation_LocationWithLocationIdAlreadyExists_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var existingLocations = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(existingLocations);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.UpdateLocationRequestFaker
            .RuleFor(r => r.LocationId, _ => existingLocations[0].LocationId)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("locationId", existingLocations[1].Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldHaveValidationErrorFor(r => r.LocationId)
            .WithErrorMessage("Location with the same Location ID already exists");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
    }

    [Fact]
    public async Task Validation_StartDateGreaterThanEndDate_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.UpdateLocationRequestFaker
            .RuleFor(r => r.StartDateTimeUtc, f => f.Date.Future())
            .RuleFor(r => r.EndDateTimeUtc, f => f.Date.Past())
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("locationId", location.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);
        //Act
        var actualResult = await _updateLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldHaveValidationErrorFor(r => r.EndDateTimeUtc)
             .WithErrorMessage("The location end date must be greater than or equal to the location start date");
    }

    [Fact]
    public async Task Validation_TypeEqualsNone_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.SaveChangesAsync();

        var updateLocationRequest = TestData.UpdateLocationRequestFaker
            .RuleFor(r => r.Type, _ => LocationType.None)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("locationId", location.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateLocationRequestValidator.TestValidateAsync(updateLocationRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldHaveValidationErrorFor(r => r.Type);
    }

    private static class TestData
    {
        public static Faker<UpdateLocationRequest> UpdateLocationRequestFaker =>
            new Faker<UpdateLocationRequest>()
                .RuleFor(u => u.Name, f => f.Company.CompanyName())
                .RuleFor(u => u.LocationId, f => f.Company.CompanySuffix())
                .RuleFor(u => u.Country, f => f.PickRandom<Country>(Country.List))
                .RuleFor(u => u.Address1,
                    (f, u) => u.Country == Country.UnitedStates ? f.Address.StreetAddress() : null)
                .RuleFor(u => u.Address2,
                    (f, u) => u.Country == Country.UnitedStates ? f.Address.SecondaryAddress() : null)
                .RuleFor(u => u.City,
                    (f, u) => u.Country == Country.UnitedStates ? f.Address.City() : null)
                .RuleFor(u => u.State,
                    (f, u) => u.Country == Country.UnitedStates ? f.PickRandom<State>() : null)
                .RuleFor(u => u.County,
                    (f, u) => u.Country == Country.UnitedStates ? f.Address.County() : null)
                .RuleFor(u => u.Zip,
                    (f, u) => u.Country == Country.UnitedStates ? f.Random.Number(10000, 999999999).ToString() : null)
                .RuleFor(u => u.Address,
                    (f, u) => u.Country == Country.International ? f.Address.FullAddress() : null)
                .RuleFor(u => u.Type, f => f.PickRandomWithout(LocationType.None));

        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static Faker<Account> AccountFaker =>
            new Faker<Account>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Country, f => f.Address.Country());

        public static Faker<Location> LocationFaker =>
            new Faker<Location>()
                .RuleFor(l => l.Id, _ => Guid.NewGuid())
                .RuleFor(l => l.Name, f => f.Lorem.Word())
                .RuleFor(l => l.LocationId, _ => Guid.NewGuid().ToString())
                .RuleFor(l => l.Country, _ => Country.UnitedStates.Name)
                .RuleFor(l => l.State, f => f.PickRandom<State>())
                .RuleFor(l => l.County, f => f.Address.County())
                .RuleFor(l => l.City, f => f.Address.City())
                .RuleFor(t => t.Type, t => t.PickRandom<LocationType>())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(t => t.Status, _ => Status.Active);
    }
}

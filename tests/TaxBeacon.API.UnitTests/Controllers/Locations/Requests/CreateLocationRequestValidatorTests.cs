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

namespace TaxBeacon.API.UnitTests.Controllers.Locations.Requests;

public sealed class CreateLocationRequestValidatorTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly CreateLocationRequestValidator _createLocationRequestValidator;
    private readonly Mock<INaicsService> _naicsServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public CreateLocationRequestValidatorTests()
    {
        _currentUserServiceMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(CreateLocationRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _naicsServiceMock = new();

        _httpContextAccessorMock = new();

        _createLocationRequestValidator = new CreateLocationRequestValidator(
            _httpContextAccessorMock.Object,
            _dbContextMock,
            _currentUserServiceMock.Object,
            _naicsServiceMock.Object);

    }

    [Fact]
    public async Task Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.CreateLocationRequestFaker
            .RuleFor(r => r.EntitiesIds, _ => entities.Select(e => e.Id))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _createLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.EntitiesIds);
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
        var entities = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.CreateLocationRequestFaker
            .RuleFor(r => r.Name, _ => name)
            .RuleFor(r => r.EntitiesIds, _ => entities.Select(e => e.Id))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _createLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.EntitiesIds);
    }

    [Fact]
    public async Task Validation_LongName_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.CreateLocationRequestFaker
            .RuleFor(r => r.Name, f => f.Random.String2(200))
            .RuleFor(r => r.EntitiesIds, _ => entities.Select(e => e.Id))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _createLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("The location name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.EntitiesIds);
    }

    [Fact]
    public async Task Validation_LocationWithNameAlreadyExists_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate(2);
        var existingLocation = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.Locations.AddRangeAsync(existingLocation);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("accountId", account.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var createUserRequest = TestData.CreateLocationRequestFaker
            .RuleFor(r => r.Name, _ => existingLocation.Name)
            .RuleFor(r => r.EntitiesIds, _ => entities.Select(e => e.Id))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _createLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("Location with the same name already exists");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.EntitiesIds);
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
        var entities = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.CreateLocationRequestFaker
            .RuleFor(r => r.LocationId, _ => locationId)
            .RuleFor(r => r.EntitiesIds, _ => entities.Select(e => e.Id))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _createLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.EntitiesIds);
    }

    [Fact]
    public async Task Validation_LongLocationId_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.CreateLocationRequestFaker
            .RuleFor(r => r.LocationId, f => f.Random.String2(200))
            .RuleFor(r => r.EntitiesIds, _ => entities.Select(e => e.Id))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _createLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldHaveValidationErrorFor(r => r.LocationId)
            .WithErrorMessage("The location id must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.EntitiesIds);
    }

    [Fact]
    public async Task Validation_LocationWithLocationIdAlreadyExists_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate(2);
        var existingLocation = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.Locations.AddRangeAsync(existingLocation);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.CreateLocationRequestFaker
            .RuleFor(r => r.LocationId, _ => existingLocation.LocationId)
            .RuleFor(r => r.EntitiesIds, _ => entities.Select(e => e.Id))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _createLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldHaveValidationErrorFor(r => r.LocationId)
            .WithErrorMessage("Location with the same location ID already exists");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.EntitiesIds);
    }

    [Fact]
    public async Task Validation_EntitiesIdsListIsEmpty_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.CreateLocationRequestFaker
            .RuleFor(r => r.EntitiesIds, _ => Enumerable.Empty<Guid>())
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _createLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldHaveValidationErrorFor(r => r.EntitiesIds);
    }

    [Fact]
    public async Task Validation_EntitiesIdsListHasDuplicatedValues_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        var entityId = Guid.NewGuid();
        var createUserRequest = TestData.CreateLocationRequestFaker
            .RuleFor(r => r.EntitiesIds, _ => new[] { entityId, entityId, entityId })
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _createLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldHaveValidationErrorFor(r => r.EntitiesIds)
            .WithErrorMessage("Entities ids items must be unique");
    }

    [Fact]
    public async Task Validation_EntitiesIdsListHasNotExistingInDbValues_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        var entityId = Guid.NewGuid();
        var createUserRequest = TestData.CreateLocationRequestFaker
            .RuleFor(r => r.EntitiesIds, _ => new[] { entityId, })
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _createLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldHaveValidationErrorFor(r => r.EntitiesIds)
            .WithErrorMessage($"Entity with Id {entityId} does not exist in the database");
    }

    [Fact]
    public async Task Validation_TypeEqualsNone_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        var createUserRequest = TestData.CreateLocationRequestFaker
            .RuleFor(r => r.Type, _ => LocationType.None)
            .RuleFor(r => r.EntitiesIds, _ => entities.Select(e => e.Id))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult = await _createLocationRequestValidator.TestValidateAsync(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LocationId);
        actualResult.ShouldHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.EntitiesIds);
    }

    private static class TestData
    {
        public static Faker<CreateLocationRequest> CreateLocationRequestFaker =>
            new Faker<CreateLocationRequest>()
                .RuleFor(e => e.Name, f => f.Company.CompanyName())
                .RuleFor(e => e.LocationId, f => f.Company.CompanySuffix())
                .RuleFor(e => e.Country, f => f.PickRandom<Country>(Country.List))
                .RuleFor(e => e.Address1,
                    (f, e) => e.Country == Country.UnitedStates ? f.Address.StreetAddress() : null)
                .RuleFor(e => e.Address2,
                    (f, e) => e.Country == Country.UnitedStates ? f.Address.SecondaryAddress() : null)
                .RuleFor(e => e.City,
                    (f, e) => e.Country == Country.UnitedStates ? f.Address.City() : null)
                .RuleFor(e => e.State,
                    (f, e) => e.Country == Country.UnitedStates ? f.PickRandom<State>() : null)
                .RuleFor(e => e.County,
                    (f, e) => e.Country == Country.UnitedStates ? f.Address.County() : null)
                .RuleFor(e => e.Zip,
                    (f, e) => e.Country == Country.UnitedStates ? f.Random.Number(10000, 999999999).ToString() : null)
                .RuleFor(e => e.Address,
                    (f, e) => e.Country == Country.International ? f.Address.FullAddress() : null)
                .RuleFor(e => e.Type, f => f.PickRandomWithout(LocationType.None));

        public static readonly Faker<Entity> EntityFaker =
            new Faker<Entity>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.EntityId, f => Guid.NewGuid().ToString())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Type, _ => AccountEntityType.LLC.Name)
                .RuleFor(t => t.City, f => f.Address.City())
                .RuleFor(t => t.State, _ => State.NM)
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(t => t.Status, _ => Status.Active)
                .RuleFor(t => t.Country, f => f.PickRandom<Country>(Country.List).ToString());

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
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
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

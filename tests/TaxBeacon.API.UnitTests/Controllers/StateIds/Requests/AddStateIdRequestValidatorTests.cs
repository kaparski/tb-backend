using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.API.Controllers.StateIds.Requests;
using TaxBeacon.API.UnitTests.Controllers.Locations.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.StateIds.Requests;

public sealed class AddStateIdRequestValidatorTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly AddStateIdsRequestValidator _addStateIdsRequestValidator;

    public AddStateIdRequestValidatorTests()
    {
        _currentUserServiceMock = new();
        _httpContextAccessorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(CreateLocationRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _addStateIdsRequestValidator = new AddStateIdsRequestValidator(
            _httpContextAccessorMock.Object,
            _dbContextMock,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_ValidRequest_ShouldHaveNoErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("entityId", entity.Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var stateIdRequest = TestData.StateIdRequestFaker.Generate();
        var addStateIdRequest = new AddStateIdsRequest { StateIdsToAdd = new List<StateIdRequest> { stateIdRequest } };

        // Act
        var actualResult = await _addStateIdsRequestValidator.TestValidateAsync(addStateIdRequest);

        // Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidateAsync_StateIdsToAddIsEmpty_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("entityId", entity.Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var addStateIdRequest = new AddStateIdsRequest { StateIdsToAdd = Enumerable.Empty<StateIdRequest>().ToList() };

        // Act
        var actualResult = await _addStateIdsRequestValidator.TestValidateAsync(addStateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(r => r.StateIdsToAdd);
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task ValidateAsync_StateIdsToAddHasDuplicatedStates_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("entityId", entity.Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var stateIdRequests = TestData.StateIdRequestFaker
            .RuleFor(r => r.State, _ => State.AK)
            .Generate(2);
        var addStateIdRequest = new AddStateIdsRequest { StateIdsToAdd = stateIdRequests };

        // Act
        var actualResult = await _addStateIdsRequestValidator.TestValidateAsync(addStateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(r => r.StateIdsToAdd)
                .WithErrorMessage("State should be unique");
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task ValidateAsync_StateIdsToAddHasDuplicatedStateIdCodes_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("entityId", entity.Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var stateIdRequests = TestData.StateIdRequestFaker
            .RuleFor(r => r.StateIdCode, _ => "1")
            .Generate(2);
        var addStateIdRequest = new AddStateIdsRequest { StateIdsToAdd = stateIdRequests };

        // Act
        var actualResult = await _addStateIdsRequestValidator.TestValidateAsync(addStateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(r => r.StateIdsToAdd)
                .WithErrorMessage("StateId Code should be unique");
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task ValidateAsync_StateIdWithTheSameStateAlreadyExistsInDbForThisEntity_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate();

        var stateIdRequest = TestData.StateIdRequestFaker
            .Generate();

        var existingStateId = stateIdRequest.Adapt<StateId>();
        existingStateId.EntityId = entity.Id;
        existingStateId.TenantId = tenant.Id;
        existingStateId.StateIdCode = "1";

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.StateIds.AddAsync(existingStateId);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("entityId", entity.Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var addStateIdRequest = new AddStateIdsRequest { StateIdsToAdd = new() { stateIdRequest } };

        // Act
        var actualResult = await _addStateIdsRequestValidator.TestValidateAsync(addStateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor("StateIdsToAdd[0].State")
                .WithErrorMessage("StateId with the same state already exists");
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task ValidateAsync_StateIdWithTheSameStateIdCodeAlreadyExistsInDb_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate(2);

        var stateIdRequest = TestData.StateIdRequestFaker
            .Generate();

        var existingStateId = stateIdRequest.Adapt<StateId>();
        existingStateId.EntityId = entities[0].Id;
        existingStateId.TenantId = tenant.Id;

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.StateIds.AddAsync(existingStateId);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("entityId", entities[^1].Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var addStateIdRequest = new AddStateIdsRequest { StateIdsToAdd = new() { stateIdRequest } };

        // Act
        var actualResult = await _addStateIdsRequestValidator.TestValidateAsync(addStateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor("StateIdsToAdd[0].StateIdCode")
                .WithErrorMessage("Entity with the same State ID code already exists");
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task ValidateAsync_StateIdWithTheSameStateIdCodeExistsInAnotherTenant_ShouldHaveNoErrors()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var accounts = TestData.AccountFaker.Generate(2);
        var entities = TestData.EntityFaker
            .Generate(2);

        accounts[0].TenantId = tenants[0].Id;
        accounts[^1].TenantId = tenants[^1].Id;
        entities[0].TenantId = tenants[0].Id;
        entities[0].AccountId = accounts[0].Id;
        entities[^1].TenantId = tenants[^1].Id;
        entities[^1].AccountId = accounts[^1].Id;

        var existingStateIds = TestData.StateIdRequestFaker
            .Generate(2)
            .Adapt<List<StateId>>();

        existingStateIds[0].TenantId = tenants[0].Id;
        existingStateIds[0].EntityId = entities[0].Id;
        existingStateIds[^1].TenantId = tenants[^1].Id;
        existingStateIds[^1].EntityId = entities[^1].Id;

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Accounts.AddRangeAsync(accounts);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.StateIds.AddRangeAsync(existingStateIds);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("entityId", entities[^1].Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenants[^1].Id);

        var statesAvailable = Enum.GetValues<State>().Except(existingStateIds.Select(x => x.State).Append(State.None));

        var addStateIdRequest = new AddStateIdsRequest
        {
            StateIdsToAdd = new()
            {
                TestData
                    .StateIdRequestFaker
                    .RuleFor(s => s.State, f => f.PickRandom(statesAvailable))
                    .RuleFor(s => s.StateIdCode, _ => existingStateIds[0].StateIdCode)
                    .Generate()
            }
        };

        // Act
        var actualResult = await _addStateIdsRequestValidator.TestValidateAsync(addStateIdRequest);

        // Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(25)]
    [InlineData(50)]
    public async Task ValidateAsync_RequestHasMoreItemsThanAllowed_ShouldHaveErrors(int countInDb)
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate();

        var statesValues = Enum.GetValues<State>()
            .Where(s => s != State.None)
            .ToList();
        var existingStateIds = new List<StateId>();

        for (var i = 0; i < countInDb; i++)
        {
            var dbStateId = TestData
                .StateIdRequestFaker
                .RuleFor(s => s.State, f => f.PickRandom(statesValues))
                .Generate()
                .Adapt<StateId>();
            dbStateId.Id = Guid.NewGuid();
            dbStateId.EntityId = entity.Id;
            dbStateId.TenantId = tenant.Id;
            existingStateIds.Add(dbStateId);
            statesValues.Remove(existingStateIds.Last().State);
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.StateIds.AddRangeAsync(existingStateIds);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("entityId", entity.Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var addStateIdRequest = new AddStateIdsRequest
        {
            StateIdsToAdd = statesValues
                .Select(s => TestData.StateIdRequestFaker
                    .RuleFor(stateId => stateId.State, _ => s)
                    .Generate())
                .ToList()
        };

        // Act
        var actualResult = await _addStateIdsRequestValidator.TestValidateAsync(addStateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(r => r.StateIdsToAdd)
                .WithErrorMessage($"There is a limit of 50 items for State Id, there already exists {countInDb}");
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    private static class TestData
    {
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
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.Country, f => f.Address.Country());

        public static Faker<StateIdRequest> StateIdRequestFaker =>
            new Faker<StateIdRequest>()
                .RuleFor(s => s.State, f => f.PickRandomWithout(State.None))
                .RuleFor(s => s.StateIdType, f =>
                {
                    var list = StateIdType.List.Except(new[] { StateIdType.None });
                    return f.PickRandom(list);
                })
                .RuleFor(s => s.StateIdCode, f => f.Lorem.Random.AlphaNumeric(25))
                .RuleFor(s => s.LocalJurisdiction, f => f.Lorem.Random.AlphaNumeric(100));
    }
}

using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.API.Controllers.StateIds.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.StateIds.Requests;

public sealed class UpdateStateIdRequestValidatorTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly UpdateStateIdRequestValidator _updateStateIdRequestValidator;

    public UpdateStateIdRequestValidatorTests()
    {
        _currentUserServiceMock = new();
        _httpContextAccessorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UpdateStateIdRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _updateStateIdRequestValidator = new UpdateStateIdRequestValidator(
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

        var existingStateIds = TestData.StateIdRequestFaker
            .Generate(2)
            .Adapt<List<StateId>>();

        foreach (var stateId in existingStateIds)
        {
            stateId.EntityId = entity.Id;
            stateId.TenantId = tenant.Id;
            stateId.Id = Guid.NewGuid();
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.StateIds.AddRangeAsync(existingStateIds);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("entityId", entity.Id);
        httpContext.Request.RouteValues.Add("stateId", existingStateIds[0].Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var updateStateIdRequest = TestData
            .StateIdRequestFaker
            .Generate()
            .Adapt<UpdateStateIdRequest>();

        // Act
        var actualResult = await _updateStateIdRequestValidator.TestValidateAsync(updateStateIdRequest);

        // Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
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

        var existingStateIds = TestData.StateIdRequestFaker
            .Generate(2)
            .Adapt<List<StateId>>();

        foreach (var stateId in existingStateIds)
        {
            stateId.EntityId = entity.Id;
            stateId.TenantId = tenant.Id;
            stateId.Id = Guid.NewGuid();
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.StateIds.AddRangeAsync(existingStateIds);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("entityId", entity.Id);
        httpContext.Request.RouteValues.Add("stateId", existingStateIds[0].Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var updateStateIdRequest = TestData.StateIdRequestFaker
            .RuleFor(s => s.State, _ => existingStateIds[^1].State)
            .Generate()
            .Adapt<UpdateStateIdRequest>();

        // Act
        var actualResult = await _updateStateIdRequestValidator.TestValidateAsync(updateStateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(s => s.State)
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

        var existingStateIds = TestData.StateIdRequestFaker
            .Generate(2)
            .Adapt<List<StateId>>();

        foreach (var stateId in existingStateIds)
        {
            stateId.EntityId = entities[0].Id;
            stateId.TenantId = tenant.Id;
            stateId.Id = Guid.NewGuid();
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.StateIds.AddRangeAsync(existingStateIds);
        await _dbContextMock.SaveChangesAsync();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("entityId", entities[^1].Id);
        httpContext.Request.RouteValues.Add("stateId", existingStateIds[^1].Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var updateStateIdRequest = TestData
            .StateIdRequestFaker
            .RuleFor(s => s.StateIdCode, _ => existingStateIds[0].StateIdCode)
            .Generate()
            .Adapt<UpdateStateIdRequest>();

        // Act
        var actualResult = await _updateStateIdRequestValidator.TestValidateAsync(updateStateIdRequest);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(s => s.StateIdCode)
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
        httpContext.Request.RouteValues.Add("stateId", existingStateIds[^1].Id);

        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenants[0].Id);

        var updateStateIdRequest = TestData
            .StateIdRequestFaker
            .RuleFor(s => s.StateIdCode, _ => existingStateIds[^1].StateIdCode)
            .Generate()
            .Adapt<UpdateStateIdRequest>();

        // Act
        var actualResult = await _updateStateIdRequestValidator.TestValidateAsync(updateStateIdRequest);

        // Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
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
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
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

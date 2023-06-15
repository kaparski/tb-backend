using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using OneOf.Types;
using TaxBeacon.Accounts.Services.Entities;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Entities.Accounts;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.Accounts.UnitTests.Services;
public class EntityServiceTest
{
    private readonly IAccountDbContext _accountContextMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly IEntityService _entityService;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    public EntityServiceTest()
    {
        _currentUserServiceMock = new();
        _entitySaveChangesInterceptorMock = new();
        var dbContext = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(EntityServiceTest)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _accountContextMock = dbContext;
        _dbContextMock = dbContext;
        _entityService = new EntityService(_accountContextMock, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task QueryEntities_AccountExists_ReturnsEntities()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        TestData.TestEntity.RuleFor(x => x.Tenant, tenant);
        TestData.TestAccount.RuleFor(x => x.Tenant, tenant);
        var account = TestData.TestAccount.Generate();

        TestData.TestEntity.RuleFor(x => x.Account, account);
        await _dbContextMock.Tenants.AddAsync(tenant);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        var items = TestData.TestEntity.Generate(3);
        await _accountContextMock.Entities.AddRangeAsync(items);
        await _accountContextMock.Accounts.AddRangeAsync(account);
        await _accountContextMock.SaveChangesAsync();

        // Act
        var oneOf = await _entityService.QueryEntitiesAsync(items[0].Account.Id);

        // Assert
        using (new AssertionScope())
        {
            oneOf.IsT0.Should().BeTrue();
            var result = oneOf.AsT0.Value;
            result.Should().HaveCount(3);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());
            }
        }
    }

    [Fact]
    public async Task QueryEntities_AccountDoesNotExist_ReturnsEntities()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        TestData.TestEntity.RuleFor(x => x.Tenant, tenant);
        TestData.TestAccount.RuleFor(x => x.Tenant, tenant);
        var account = TestData.TestAccount.Generate();

        TestData.TestEntity.RuleFor(x => x.Account, account);
        await _dbContextMock.Tenants.AddAsync(tenant);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        var items = TestData.TestEntity.Generate(3);
        await _accountContextMock.Entities.AddRangeAsync(items);
        await _accountContextMock.Accounts.AddRangeAsync(account);
        await _accountContextMock.SaveChangesAsync();

        // Act
        var oneOf = await _entityService.QueryEntitiesAsync(new Guid());

        // Assert
        using (new AssertionScope())
        {
            oneOf.IsT1.Should().BeTrue();
            var result = oneOf.AsT1;
            result.Should().BeOfType<NotFound>();
        }
    }

    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();

        public static readonly Faker<Entity> TestEntity =
            new Faker<Entity>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Type, t => Common.Accounts.AccountEntityType.LLC)
                .RuleFor(t => t.City, f => f.Address.City())
                .RuleFor(t => t.State, t => State.NM)
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(t => t.Status, t => Status.Active);

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => TestTenantId)
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<Account> TestAccount =
            new Faker<Account>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Website, f => f.Internet.Url())
                .RuleFor(t => t.Name, f => f.Person.FullName);
    }
}

﻿using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using OneOf.Types;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Accounts.Entities;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Permissions;
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

        var entities = TestData.TestEntity.Generate(3);
        await _accountContextMock.Entities.AddRangeAsync(entities);
        await _accountContextMock.Accounts.AddRangeAsync(account);
        await _accountContextMock.SaveChangesAsync();

        var expectedEntities = entities
            .Where(a => a.TenantId == tenant.Id)
            .ToList();

        // Act
        var actualResult = _entityService.QueryEntitiesAsync(entities[0].Account.Id).AsT0.ToList();

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveCount(expectedEntities.Count)
                .And.AllBeOfType<EntityDto>()
                .And.BeEquivalentTo(expectedEntities, opt => opt.ExcludingMissingMembers());
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
        var oneOf = _entityService.QueryEntitiesAsync(new Guid());

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
               .RuleFor(t => t.Id, f => Guid.NewGuid())
               .RuleFor(t => t.Name, f => f.Company.CompanyName())
               .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<Account> TestAccount =
            new Faker<Account>()
                .RuleFor(a => a.Id, f => Guid.NewGuid())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.Country, f => f.Address.Country());
    }
}
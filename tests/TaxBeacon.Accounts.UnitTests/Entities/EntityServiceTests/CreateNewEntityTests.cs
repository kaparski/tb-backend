using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Entities;
public partial class EntityServiceTests
{
    [Fact]
    public async Task CreateNewEntity_CreateEntityDtoAndNoConflictErrors_ReturnsEntityDetailsDto()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        var createEntityDto = TestData.CreateEntityFaker.Generate();
        var expectedEntity = createEntityDto.Adapt<Entity>();
        expectedEntity.TenantId = tenant.Id;
        expectedEntity.AccountId = account.Id;
        expectedEntity.Status = Status.Active;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.CreateNewEntityAsync(account.Id, createEntityDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var createdEntity, out _).Should().BeTrue();
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            createdEntity.Should().BeEquivalentTo(expectedEntity, opt =>
            {
                opt.Excluding(x => x.Id);
                opt.Excluding(x => x.Phones);
                opt.ExcludingMissingMembers();
                return opt;
            });
            createdEntity.Phones.Should().BeEquivalentTo(createEntityDto.Phones, opt =>
            {
                opt.WithStrictOrdering();
                opt.Excluding(x => x.Id);
                opt.ExcludingMissingMembers();
                return opt;
            });
        }
    }

    [Fact]
    public async Task CreateNewEntity_AccountNotExistsInTenant_ReturnsNotFound()
    {
        // Act
        var tenants = TestData.TenantFaker.Generate(2);
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenants[0].Id)
            .Generate();

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[^1].Id);

        // Act
        var actualResult = await _entityService.CreateNewEntityAsync(account.Id, new CreateEntityDto());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT2.Should().BeFalse();
        }
    }

    [Fact]
    public async Task CreateNewEntity_AccountNotExistsInDb_ReturnsNotFound()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.CreateNewEntityAsync(Guid.NewGuid(), new CreateEntityDto());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT2.Should().BeFalse();
        }
    }

    [Fact]
    public async Task CreateNewEntity_EntityWithSuchNameAlreadyExists_ReturnsInvalidOperation()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        var createEntityDto = TestData.CreateEntityFaker
            .RuleFor(e => e.Name, _ => entity.Name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.CreateNewEntityAsync(account.Id, createEntityDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT2(out var invalidOperation, out _).Should().BeTrue();
            invalidOperation.Should().BeEquivalentTo(new InvalidOperation(
                $"Entity with the same name {createEntityDto.Name} already exists",
                nameof(createEntityDto.Name)));
        }
    }

    [Fact]
    public async Task CreateNewEntity_EntityWithSuchFeinAlreadyExists_ReturnsInvalidOperation()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.Fein, f => f.Random.Number(10000, 999999999).ToString())
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        var createEntityDto = TestData.CreateEntityFaker
            .RuleFor(e => e.Fein, _ => entity.Fein)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.CreateNewEntityAsync(account.Id, createEntityDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT2(out var invalidOperation, out _).Should().BeTrue();
            invalidOperation.Should().BeEquivalentTo(new InvalidOperation(
                $"Entity with the same FEIN '{createEntityDto.Fein}' already exists",
                nameof(createEntityDto.Fein)));
        }
    }

    [Fact]
    public async Task CreateNewEntity_EntityWithSuchEinAlreadyExists_ReturnsInvalidOperation()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.Ein, f => f.Random.Number(10000, 999999999).ToString())
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        var createEntityDto = TestData.CreateEntityFaker
            .RuleFor(e => e.Ein, _ => entity.Ein)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.CreateNewEntityAsync(account.Id, createEntityDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT2(out var invalidOperation, out _).Should().BeTrue();
            invalidOperation.Should().BeEquivalentTo(new InvalidOperation(
                $"Entity with the same EIN '{createEntityDto.Ein}' already exists",
                nameof(createEntityDto.Ein)));
        }
    }
}

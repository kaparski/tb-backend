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
    public async Task UpdateEntity_AccountNotExistsInDb_ReturnsNotFound()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.UpdateEntityAsync(Guid.NewGuid(), new UpdateEntityDto());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT2.Should().BeFalse();
        }
    }

    [Fact]
    public async Task UpdateEntity_EntityWithSuchNameAlreadyExists_ReturnsInvalidOperation()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .Generate(2);

        var entityFirst = entities[0];
        var entitySecond = entities[1];

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        var updateEntityDto = TestData.UpdateEntityFaker
            .RuleFor(e => e.Name, _ => entitySecond.Name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.UpdateEntityAsync(entityFirst.Id, updateEntityDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT2(out var invalidOperation, out _).Should().BeTrue();
            invalidOperation.Should().BeEquivalentTo(new InvalidOperation(
                $"Entity with the same name {updateEntityDto.Name} already exists",
                nameof(updateEntityDto.Name)));
        }
    }

    [Fact]
    public async Task UpdateEntity_EntityWithSuchFeinAlreadyExists_ReturnsInvalidOperation()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.Fein,
                (f) => f.Random.Number(10000, 999999999).ToString())
            .Generate(2);

        var entityFirst = entities[0];
        var entitySecond = entities[1];

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        var updateEntityDto = TestData.UpdateEntityFaker
            .RuleFor(e => e.Fein, _ => entitySecond.Fein)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.UpdateEntityAsync(entityFirst.Id, updateEntityDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT2(out var invalidOperation, out _).Should().BeTrue();
            invalidOperation.Should().BeEquivalentTo(new InvalidOperation(
                $"Entity with the same FEIN '{updateEntityDto.Fein}' already exists",
                nameof(updateEntityDto.Fein)));
        }
    }

    [Fact]
    public async Task UpdateEntity_EntityWithSuchEinAlreadyExists_ReturnsInvalidOperation()
    {
        // Act
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.Ein, f => f.Random.Number(10000, 999999999).ToString())
            .Generate(2);

        var entityFirst = entities[0];
        var entitySecond = entities[1];

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        var updateEntityDto = TestData.UpdateEntityFaker
            .RuleFor(e => e.Ein, _ => entitySecond.Ein)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.UpdateEntityAsync(entityFirst.Id, updateEntityDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT2(out var invalidOperation, out _).Should().BeTrue();
            invalidOperation.Should().BeEquivalentTo(new InvalidOperation(
                $"Entity with the same EIN '{updateEntityDto.Ein}' already exists",
                nameof(updateEntityDto.Ein)));
        }
    }

    [Fact]
    public async Task UpdateEntity_UpdateEntityDtoAndNoConflictErrors_ReturnsEntityDetailsDto()
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

        var updateEntityDto = TestData.UpdateEntityFaker.Generate();
        var expectedEntity = updateEntityDto.Adapt<Entity>();
        expectedEntity.Status = Status.Active;
        expectedEntity.TenantId = tenant.Id;
        expectedEntity.AccountId = account.Id;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.UpdateEntityAsync(entity.Id, updateEntityDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var createdEntity, out _).Should().BeTrue();
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            createdEntity.Should().BeEquivalentTo(expectedEntity, opt =>
            {
                opt.Excluding(x => x.Id);
                opt.Excluding(x => x.CreatedDateTimeUtc);
                opt.Excluding(x => x.Phones);
                opt.ExcludingMissingMembers();
                return opt;
            });
            createdEntity.Phones.Should().BeEquivalentTo(updateEntityDto.Phones, opt => opt.ExcludingMissingMembers());
        }
    }
}

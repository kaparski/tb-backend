using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Entities;
public partial class EntityServiceTests
{
    [Fact]
    public async Task AddStateIds_StateIdsAndEntityExistsInDb_ReturnsStateIds()
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

        var addStateIdDtos = TestData.AddStateIdFaker
            .Generate(3);
        var expectedStateIds = addStateIdDtos.Adapt<List<StateId>>();
        foreach (var e in expectedStateIds)
        {
            e.TenantId = tenant.Id;
            e.EntityId = entity.Id;
        }

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.AddStateIdsAsync(entity.Id, addStateIdDtos);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var stateIds, out _).Should().BeTrue();
            stateIds.Should().BeEquivalentTo(expectedStateIds, opt =>
            {
                opt.Excluding(x => x.Id);
                opt.ExcludingMissingMembers();
                return opt;
            });
        }
    }

    [Fact]
    public async Task AddStateIds_AddStateIdsEntityNotExistsInDb_ReturnsNotFound()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService
            .AddStateIdsAsync(Guid.NewGuid(), new List<AddStateIdDto>());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task AddStateIds_AddStateIdsEntityNotExistsInTenant_ReturnsNotFound()
    {
        // Act
        var tenants = TestData.TenantFaker.Generate(2);
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenants[0].Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenants[0].Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[^1].Id);

        // Act
        var actualResult = await _entityService
            .AddStateIdsAsync(entity.Id, new List<AddStateIdDto>());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }
}

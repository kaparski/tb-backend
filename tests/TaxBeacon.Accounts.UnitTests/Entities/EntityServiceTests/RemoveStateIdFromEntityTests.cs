using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.Accounts.UnitTests.Entities;
public partial class EntityServiceTests
{
    [Fact]
    public async Task RemoveStateIdFromEntity_StateIdAssignedToEntity_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .Generate();

        var entity = TestData.EntityFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.AccountId, _ => account.Id)
            .Generate();

        var stateIds = TestData.StateIdFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.Tenant, _ => tenant)
            .RuleFor(x => x.Entity, _ => entity)
            .Generate(8);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entity);
        await _dbContextMock.StateIds.AddRangeAsync(stateIds);
        await _dbContextMock.SaveChangesAsync();
        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.RemoveStateIdFromEntityAsync(entity.Id, stateIds[0].Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeTrue();
            _dbContextMock.StateIds.Should().HaveCount(stateIds.Count - 1)
                .And.NotContain(stateIds[0]);

            var actualActivityLog = await _dbContextMock.EntityActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(EntityEventType.EntityStateIdDeleted);
            actualActivityLog?.EntityId.Should().Be(entity.Id);
        }
    }

    [Fact]
    public async Task RemoveStateIdFromEntity_StateIdIsNotAssignedToEntity_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .Generate();

        var entities = TestData.EntityFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.AccountId, _ => account.Id)
            .Generate(2);

        var stateIds = TestData.StateIdFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.Tenant, _ => tenant)
            .RuleFor(x => x.Entity, _ => entities[0])
            .Generate(8);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.StateIds.AddRangeAsync(stateIds);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.RemoveStateIdFromEntityAsync(entities[1].Id, stateIds[0].Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            _dbContextMock.StateIds.Should().HaveCount(stateIds.Count);
        }
    }

    [Fact]
    public async Task RemoveStateIdFromEntity_StateIdNotExists_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .Generate();

        var entity = TestData.EntityFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.AccountId, _ => account.Id)
            .Generate();

        var stateIds = TestData.StateIdFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.Tenant, _ => tenant)
            .RuleFor(x => x.Entity, _ => entity)
            .Generate(8);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entity);
        await _dbContextMock.StateIds.AddRangeAsync(stateIds);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.RemoveStateIdFromEntityAsync(entity.Id, new Guid());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            _dbContextMock.StateIds.Should().HaveCount(stateIds.Count);
        }
    }
}

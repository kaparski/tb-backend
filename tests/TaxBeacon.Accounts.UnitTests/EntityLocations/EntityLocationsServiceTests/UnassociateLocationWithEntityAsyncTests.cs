using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.Accounts.UnitTests.EntityLocations;
public partial class EntityLocationsServiceTests
{
    [Fact]
    public async Task UnassociateLocationWithEntityAsync_LocationAssignedToEntity_ReturnsSuccess()
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

        var location = TestData.LocationFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.Tenant, _ => tenant)
            .Generate();

        var entityLocation = TestData.EntityLocationFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.LocationId, _ => location.Id)
            .RuleFor(x => x.EntityId, _ => entity.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entity);
        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.EntityLocations.AddAsync(entityLocation);
        await _dbContextMock.SaveChangesAsync();
        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityLocationsService.UnassociateLocationWithEntityAsync(entity.Id, location.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeTrue();
            _dbContextMock.EntityLocations.Should().HaveCount(0)
                .And.NotContain(entityLocation);

            var actualEntitiesActivityLog = await _dbContextMock.EntityActivityLogs.LastOrDefaultAsync();
            actualEntitiesActivityLog.Should().NotBeNull();
            actualEntitiesActivityLog?.Date.Should().Be(currentDate);
            actualEntitiesActivityLog?.EventType.Should().Be(EntityEventType.LocationUnassociated);
            actualEntitiesActivityLog?.EntityId.Should().Be(entity.Id);

            var actualLocationsActivityLog = await _dbContextMock.LocationActivityLogs.LastOrDefaultAsync();
            actualLocationsActivityLog.Should().NotBeNull();
            actualLocationsActivityLog?.Date.Should().Be(currentDate);
            actualLocationsActivityLog?.EventType.Should().Be(LocationEventType.EntityUnassociated);
            actualLocationsActivityLog?.LocationId.Should().Be(location.Id);
        }
    }

    [Fact]
    public async Task UnassociateLocationWithEntityAsync_LocationIsNotAssignedToEntity_ReturnsNotFound()
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

        var location = TestData.LocationFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.Tenant, _ => tenant)
            .Generate();

        var entityLocation = TestData.EntityLocationFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.LocationId, _ => location.Id)
            .RuleFor(x => x.EntityId, _ => entities[0].Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.Locations.AddAsync(location);
        await _dbContextMock.EntityLocations.AddAsync(entityLocation);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult =
            await _entityLocationsService.UnassociateLocationWithEntityAsync(entities[1].Id, location.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            _dbContextMock.EntityLocations.Should().HaveCount(1);
        }
    }

    [Fact]
    public async Task UnassociateLocationWithEntityAsync_EntityIsNotAssignedToLocation_ReturnsNotFound()
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

        var location = TestData.LocationFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.Tenant, _ => tenant)
            .Generate(8);

        var entityLocation = TestData.EntityLocationFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.LocationId, _ => location[1].Id)
            .RuleFor(x => x.EntityId, _ => entity.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entity);
        await _dbContextMock.Locations.AddRangeAsync(location);
        await _dbContextMock.EntityLocations.AddAsync(entityLocation);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityLocationsService.UnassociateLocationWithEntityAsync(entity.Id, new Guid());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            _dbContextMock.Locations.Should().HaveCount(location.Count);
        }
    }
}

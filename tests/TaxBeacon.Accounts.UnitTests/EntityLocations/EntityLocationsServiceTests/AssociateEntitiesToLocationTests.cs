using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.Accounts.UnitTests.EntityLocations;
public partial class EntityLocationsServiceTests
{
    [Fact]
    public async Task AssociateEntitiesToLocation_LocationExists_ReturnsSuccess()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate(3);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(location);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);
        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock.Setup(x => x.UtcNow).Returns(currentDate);

        //Act
        var result = await _entityLocationsService.AssociateEntitiesToLocation(account.Id, location.Id,
            entities.Select(x => x.Id).ToList(), default);

        //Assert
        using (new AssertionScope())
        {
            result.IsT0.Should().BeTrue();

            _dbContextMock.EntityLocations.Select(x => new { x.EntityId, x.LocationId })
                .Should().BeEquivalentTo(entities.Select(x => new { EntityId = x.Id, LocationId = location.Id }));

            var actualEntitiesActivityLog = await _dbContextMock.EntityActivityLogs.ToListAsync();
            actualEntitiesActivityLog.Should().AllSatisfy(x =>
            {
                x.Should().NotBeNull();
                if (x != null)
                {
                    x.Date.Should().Be(currentDate);
                    x.EventType.Should().Be(EntityEventType.LocationAssociated);
                    entities.Should().Contain(l => l.Id == x.EntityId);
                }
            }).And.HaveCount(entities.Count);

            var actualLocationActivityLog = await _dbContextMock.LocationActivityLogs.FirstOrDefaultAsync();
            actualLocationActivityLog.Should().NotBeNull();
            actualLocationActivityLog?.Date.Should().Be(currentDate);
            actualLocationActivityLog?.EventType.Should().Be(LocationEventType.EntityAssociated);
            actualLocationActivityLog?.LocationId.Should().Be(location.Id);
        }
    }

    [Fact]
    public async Task AssociateEntitiesToLocation_LocationDoesntExist_ReturnsNotFound()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .RuleFor(a => a.AccountId, f => f.Random.String(10))
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .Generate(3);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(location);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        //Act
        var result = await _entityLocationsService.AssociateEntitiesToLocation(account.Id, new Guid(),
            entities.Select(x => x.Id).ToList(), default);

        //Assert
        result.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task AssociateEntitiesToLocation_LocationNotLinkedToAccount_ReturnsNotFound()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var location = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => new Guid())
            .Generate();
        var entities = TestData.EntityFaker
            .Generate(3);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(location);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        //Act
        var result = await _entityLocationsService.AssociateEntitiesToLocation(account.Id, location.Id,
            entities.Select(x => x.Id).ToList(), default);

        //Assert
        result.IsT1.Should().BeTrue();
    }
}

using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.Accounts.UnitTests.EntityLocations;
public partial class EntityLocationsServiceTests
{
    [Fact]
    public async Task AssociateLocationsToEntity_EntityLocationsAndAccountExist_ReturnsSuccess()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var locations = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate(3);
        var entity = TestData.EntityFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(locations);
        await _dbContextMock.Entities.AddRangeAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);
        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock.Setup(x => x.UtcNow).Returns(currentDate);

        //Act
        var result = await _entityLocationsService.AssociateLocationsToEntity(account.Id, entity.Id,
            locations.Select(x => x.Id).ToList(), default);

        //Assert
        using (new AssertionScope())
        {
            result.IsT0.Should().BeTrue();

            _dbContextMock.EntityLocations.Select(x => new { x.EntityId, x.LocationId })
                .Should().BeEquivalentTo(locations.Select(x => new { LocationId = x.Id, EntityId = entity.Id }));

            var actualEntityActivityLog = await _dbContextMock.EntityActivityLogs.FirstOrDefaultAsync();
            actualEntityActivityLog.Should().NotBeNull();
            actualEntityActivityLog?.Date.Should().Be(currentDate);
            actualEntityActivityLog?.EventType.Should().Be(EntityEventType.LocationAssociated);
            actualEntityActivityLog?.EntityId.Should().Be(entity.Id);

            var actualLocationsActivityLog = await _dbContextMock.LocationActivityLogs.ToListAsync();
            actualLocationsActivityLog.Should().AllSatisfy(x =>
            {
                x.Should().NotBeNull();
                if (x != null)
                {
                    x.Date.Should().Be(currentDate);
                    x.EventType.Should().Be(LocationEventType.EntityAssociated);
                    locations.Should().Contain(l => l.Id == x.LocationId);
                }
            }).And.HaveCount(locations.Count);
        }
    }

    [Fact]
    public async Task AssociateLocationsToEntity_EntityDoesntExist_ReturnsNotFound()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var locations = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate(3);
        var entity = TestData.EntityFaker
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(locations);
        await _dbContextMock.Entities.AddRangeAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        //Act
        var result = await _entityLocationsService.AssociateLocationsToEntity(account.Id, new Guid(),
            locations.Select(x => x.Id).ToList(), default);

        //Assert
        result.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task AssociateLocationsToEntity_EntityNotLinkedToAccount_ReturnsNotFound()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var locations = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate(3);
        var entity = TestData.EntityFaker
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(locations);
        await _dbContextMock.Entities.AddRangeAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        //Act
        var result = await _entityLocationsService.AssociateLocationsToEntity(account.Id, entity.Id,
            locations.Select(x => x.Id).ToList(), default);

        //Assert
        result.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task AssociateLocationsToEntity_LocationNotLinkedToAccount_ReturnsNotFound()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var locations = TestData.LocationFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => new Guid())
            .Generate(3);
        var entity = TestData.EntityFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Locations.AddRangeAsync(locations);
        await _dbContextMock.Entities.AddRangeAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        //Act
        var result = await _entityLocationsService.AssociateLocationsToEntity(account.Id, entity.Id,
            locations.Select(x => x.Id).ToList(), default);

        //Assert
        result.IsT1.Should().BeTrue();
    }
}

using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Locations.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Locations;
public partial class LocationServiceTests
{
    [Fact]
    public async Task CreateNewLocation_CreateLocationDtoWithValidData_ReturnsLocationDetailsDto()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        var createLocationDto = TestData.CreateLocationFaker.Generate();
        var expectedLocation = createLocationDto.Adapt<Location>();
        expectedLocation.TenantId = tenant.Id;
        expectedLocation.AccountId = account.Id;
        expectedLocation.Status = Status.Active;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _locationService.CreateNewLocationAsync(account.Id, createLocationDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var createdLocation, out _).Should().BeTrue();
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            createdLocation.Should().BeEquivalentTo(expectedLocation, opt =>
            {
                opt.Excluding(x => x.Id);
                opt.Excluding(x => x.Phones);
                opt.ExcludingMissingMembers();
                return opt;
            });
            createdLocation.Phones.Should().BeEquivalentTo(createLocationDto.Phones, opt =>
            {
                opt.WithStrictOrdering();
                opt.Excluding(x => x.Id);
                opt.ExcludingMissingMembers();
                return opt;
            });

            var actualActivityLog = await _dbContextMock.LocationActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(LocationEventType.LocationCreated);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);
            actualActivityLog?.LocationId.Should().Be(createdLocation.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task CreateNewLocation_AccountNotExistsInTenant_ReturnsNotFound()
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
        var actualResult = await _locationService.CreateNewLocationAsync(account.Id, new CreateLocationDto());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task CreateNewLocation_AccountNotExistsInDb_ReturnsNotFound()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _locationService.CreateNewLocationAsync(Guid.NewGuid(), new CreateLocationDto());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }
}

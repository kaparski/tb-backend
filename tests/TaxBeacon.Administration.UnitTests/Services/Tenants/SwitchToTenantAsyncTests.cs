using FluentAssertions.Execution;
using FluentAssertions;
using System.Text.Json;
using TaxBeacon.Administration.Tenants.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.Administration.UnitTests.Services.Tenants;
public partial class TenantServiceTests
{
    [Fact]
    public async Task SwitchToTenantAsync_NoOldTenant_ProducesTenantEnteredEvent()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        _currentUserServiceMock
            .Setup(s => s.UserInfo)
            .Returns((user.FullName, Common.Constants.Roles.SuperAdmin));

        //Act
        var actualResult = await _tenantService.SwitchToTenantAsync(null, tenant.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.Should().NotBeNull()
                .And.BeEquivalentTo(tenant, opt => opt.ExcludingMissingMembers());

            var item = _dbContextMock.TenantActivityLogs.Single();
            var evt = JsonSerializer.Deserialize<TenantEnteredEvent>(item.Event);

            item.TenantId.Should().Be(tenant.Id);
            item.EventType.Should().Be(TenantEventType.TenantEnteredEvent);
            evt?.ExecutorId.Should().Be(user.Id);
            evt?.ExecutorFullName.Should().Be(user.FullName);
        }
    }

    [Fact]
    public async Task SwitchToTenantAsync_NoNewTenant_ProducesTenantEnteredEvent()
    {
        //Arrange
        var user = TestData.UserFaker.Generate();
        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        _currentUserServiceMock
            .Setup(s => s.UserInfo)
            .Returns((user.FullName, Common.Constants.Roles.SuperAdmin));

        //Act
        var actualResult = await _tenantService.SwitchToTenantAsync(TestData.TestTenantId, null);

        //Assert
        using (new AssertionScope())
        {
            actualResult.Should().BeNull();

            var item = _dbContextMock.TenantActivityLogs.Single();
            var evt = JsonSerializer.Deserialize<TenantExitedEvent>(item.Event);

            item.TenantId.Should().Be(TestData.TestTenantId);
            item.EventType.Should().Be(TenantEventType.TenantExitedEvent);
            evt?.ExecutorId.Should().Be(user.Id);
            evt?.ExecutorFullName.Should().Be(user.FullName);
        }
    }

    [Fact]
    public async Task SwitchToTenantAsync_ProducesBothEvents()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        _currentUserServiceMock
            .Setup(s => s.UserInfo)
            .Returns((user.FullName, Common.Constants.Roles.SuperAdmin));

        var oldTenantId = Guid.NewGuid();

        //Act
        var actualResult = await _tenantService.SwitchToTenantAsync(oldTenantId, tenant.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.Should().NotBeNull()
                .And.BeEquivalentTo(tenant, opt => opt.ExcludingMissingMembers());

            var items = _dbContextMock.TenantActivityLogs.OrderBy(a => a.Date).ToList();
            items.Should().HaveCount(2);

            var exitEvt = JsonSerializer.Deserialize<TenantEnteredEvent>(items[0].Event);
            var enterEvt = JsonSerializer.Deserialize<TenantEnteredEvent>(items[1].Event);

            items[0].TenantId.Should().Be(oldTenantId);
            items[0].EventType.Should().Be(TenantEventType.TenantExitedEvent);
            exitEvt?.ExecutorId.Should().Be(user.Id);
            exitEvt?.ExecutorFullName.Should().Be(user.FullName);

            items[1].TenantId.Should().Be(tenant.Id);
            items[1].EventType.Should().Be(TenantEventType.TenantEnteredEvent);
            enterEvt?.ExecutorId.Should().Be(user.Id);
            enterEvt?.ExecutorFullName.Should().Be(user.FullName);
        }
    }
}

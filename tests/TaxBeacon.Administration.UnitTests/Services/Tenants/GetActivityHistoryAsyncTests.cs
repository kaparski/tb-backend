using FluentAssertions.Execution;
using FluentAssertions;
using System.Text.Json;
using TaxBeacon.Administration.Tenants.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Tenants;
public partial class TenantServiceTests
{
    [Fact]
    public async Task GetActivityHistoryAsync_TenantExists_ReturnListOfActivityLogsInDescendingOrderByDate()
    {
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var activities = new[]
        {
            new TenantActivityLog
            {
                Date = new DateTime(2000, 01, 1),
                TenantId = tenant.Id,
                EventType = TenantEventType.TenantEnteredEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new TenantEnteredEvent(
                        user.Id,
                        "Super Admin",
                        user.FullName,
                        DateTime.UtcNow
                    )
                )
            },
            new TenantActivityLog
            {
                Date = new DateTime(2000, 01, 2),
                TenantId = tenant.Id,
                EventType = TenantEventType.TenantUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new TenantUpdatedEvent(
                        user.Id,
                        "Super Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "",
                        ""
                    )
                )
            },
            new TenantActivityLog
            {
                Date = new DateTime(2000, 01, 3),
                TenantId = tenant.Id,
                EventType = TenantEventType.TenantExitedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new TenantExitedEvent(
                        user.Id,
                        "Super Admin",
                        user.FullName,
                        DateTime.UtcNow
                    )
                )
            }
        };

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TenantActivityLogs.AddRangeAsync(activities);
        await _dbContextMock.SaveChangesAsync();

        var actualResult = await _tenantService.GetActivityHistoryAsync(tenant.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(3);
            activitiesResult.Query.Should().BeInDescendingOrder(x => x.Date);
        }
    }

    [Fact]
    public async Task GetActivityHistoryAsync_TenantDoesNotExist_ReturnsNotFound()
    {
        var tenant = TestData.TenantFaker.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var actualResult = await _tenantService.GetActivityHistoryAsync(Guid.NewGuid());

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var _, out var notFound);
            notFound.Should().NotBeNull();
        }
    }
}

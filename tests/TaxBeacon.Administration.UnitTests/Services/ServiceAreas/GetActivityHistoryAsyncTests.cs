using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.Administration.ServiceAreas.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration.Entities;
using System.Text.Json;

namespace TaxBeacon.Administration.UnitTests.Services.ServiceAreas;
public partial class ServiceAreaServiceTests
{
    [Fact]
    public async Task GetActivityHistoryAsync_ServiceAreaExistsAndTenantIdEqualsUserTenantId_ReturnsListOfActivityLogsInDescendingOrderByDate()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();
        var user = TestData.TestUser.Generate();
        var activities = new[]
        {
            new ServiceAreaActivityLog
            {
                Date = new DateTime(2000, 01, 1),
                TenantId = serviceArea.TenantId,
                ServiceAreaId = serviceArea.Id,
                EventType = ServiceAreaEventType.ServiceAreaUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new ServiceAreaUpdatedEvent(
                        user.Id,
                        "Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "Old",
                        "New"
                    )
                )
            },
            new ServiceAreaActivityLog
            {
                Date = new DateTime(2000, 01, 2),
                TenantId = serviceArea.TenantId,
                ServiceAreaId = serviceArea.Id,
                EventType = ServiceAreaEventType.ServiceAreaUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new ServiceAreaUpdatedEvent(
                        user.Id,
                        "Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "Old",
                        "New"
                    )
                )
            },
            new ServiceAreaActivityLog
            {
                Date = new DateTime(2000, 01, 3),
                TenantId = serviceArea.TenantId,
                ServiceAreaId = serviceArea.Id,
                EventType = ServiceAreaEventType.ServiceAreaUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new ServiceAreaUpdatedEvent(
                        user.Id,
                        "Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "Old",
                        "New"
                    )
                )
            }
        };

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.ServiceAreaActivityLogs.AddRangeAsync(activities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.GetActivityHistoryAsync(serviceArea.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(3);
            activitiesResult.Query.Should().BeInDescendingOrder(x => x.Date);
        }
    }
}

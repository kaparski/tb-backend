using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.Administration.Departments.Activities.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration.Entities;
using System.Text.Json;

namespace TaxBeacon.Administration.UnitTests.Services.Departments;
public partial class DepartmentServiceTests
{
    [Fact]
    public async Task GetActivityHistoryAsync_TenantExists_ReturnListOfActivityLogsInDescendingOrderByDate()
    {
        var tenant = TestData.TestTenant.Generate();
        var department = TestData.TestDepartment.Generate();
        var user = TestData.TestUser.Generate();
        var activities = new[]
        {
            new DepartmentActivityLog
            {
                Date = new DateTime(2000, 1, 2),
                TenantId = tenant.Id,
                DepartmentId = department.Id,
                EventType = DepartmentEventType.DepartmentUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new DepartmentUpdatedEvent(
                        user.Id,
                        "Super Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "",
                        ""
                    )
                )
            },
            new DepartmentActivityLog
            {
                Date = new DateTime(2001, 2, 3),
                TenantId = tenant.Id,
                DepartmentId = department.Id,
                EventType = DepartmentEventType.DepartmentUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new DepartmentUpdatedEvent(
                        user.Id,
                        "Super Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "",
                        ""
                    )
                )
            },
        };

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Departments.AddAsync(department);
        await _dbContextMock.DepartmentActivityLogs.AddRangeAsync(activities);
        await _dbContextMock.SaveChangesAsync();

        var actualResult = await _departmentService.GetActivityHistoryAsync(department.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(2);
            activitiesResult.Query.Should().BeInDescendingOrder(x => x.Date);
        }
    }
}

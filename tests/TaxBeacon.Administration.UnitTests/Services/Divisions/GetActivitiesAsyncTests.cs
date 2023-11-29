using FluentAssertions.Execution;
using FluentAssertions;
using Moq;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Divisions;
public partial class DivisionsServiceTests
{
    [Fact]
    public async Task GetActivitiesAsync_DivisionExists_ShouldCallAppropriateFactory()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;

        var divisionActivity = new DivisionActivityLog()
        {
            Date = DateTime.UtcNow,
            TenantId = TenantId,
            Division = division,
            EventType = DivisionEventType.None,
            Revision = 1
        };

        _dbContextMock.DivisionActivityLogs.Add(divisionActivity);
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _divisionsService.GetActivitiesAsync(division.Id);

        //Assert

        _divisionActivityFactory.Verify(x => x.Create(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetActivitiesAsync_DivisionDoesNotExistWithinCurrentTenant_ShouldReturnNotFound()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var division = TestData.TestDivision.Generate();
        division.Tenant = tenant;
        _dbContextMock.Tenants.Add(tenant);
        _dbContextMock.Divisions.Add(division);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _divisionsService.GetActivitiesAsync(division.Id);

        //Assert
        resultOneOf.TryPickT1(out _, out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetActivitiesAsync_DivisionExists_ShouldReturnExpectedNumberOfItems()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;

        var activities = new[]
        {
            new DivisionActivityLog()
            {
                Date = new DateTime(2000, 01, 1),
                TenantId = TenantId,
                Division = division,
                EventType = DivisionEventType.None,
                Revision = 1
            },
        };

        _dbContextMock.DivisionActivityLogs.AddRange(activities);
        await _dbContextMock.SaveChangesAsync();

        const int pageSize = 2;

        //Act
        var resultOneOf = await _divisionsService.GetActivitiesAsync(division.Id, 1, pageSize);

        //Assert
        using (new AssertionScope())
        {
            resultOneOf.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(1);
        }
    }
}

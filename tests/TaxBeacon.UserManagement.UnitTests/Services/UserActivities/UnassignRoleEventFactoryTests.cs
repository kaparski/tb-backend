using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Factories;
using TaxBeacon.Administration.Users.Activities.Models;

namespace TaxBeacon.UserManagement.UnitTests.Services.UserActivities;

public class UnassignRoleEventFactoryTests
{
    private readonly IUserActivityFactory _sut;

    public UnassignRoleEventFactoryTests() => _sut = new UnassignRolesEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var unassignedByUserId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var unassignRolesEvent = new UnassignRolesEvent("Admin",
            date,
            unassignedByUserId,
            "Test Full Name",
            "TestRole");

        //Act
        var result = _sut.Create(JsonSerializer.Serialize(unassignRolesEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test Full Name");
            result.Message.Should().Be("User unassigned from the following role(s): Admin");
        };

    }
}

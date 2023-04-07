using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services.Activities;

namespace TaxBeacon.UserManagement.UnitTests.Services.UserActivities;

public class UnassignRoleEventFactoryTests
{
    private readonly IUserActivityFactory _sut;

    public UnassignRoleEventFactoryTests() => _sut = new AssignRolesEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var unassignedByUserId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var unassignRolesEvent = new UnassignRolesEvent("Admin",
            date,
            unassignedByUserId,
            "Test",
            "Test");

        //Act
        var result = _sut.Create(JsonSerializer.Serialize(unassignRolesEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("User has been unassigned from the following role(s): Admin");
        };

    }
}

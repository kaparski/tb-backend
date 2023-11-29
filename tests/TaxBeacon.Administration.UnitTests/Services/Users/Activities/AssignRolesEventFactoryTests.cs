using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Factories;
using TaxBeacon.Administration.Users.Activities.Models;

namespace TaxBeacon.Administration.UnitTests.Services.Users.Activities;

public sealed class AssignRolesEventFactoryTests
{
    private readonly IUserActivityFactory _sut;

    public AssignRolesEventFactoryTests() => _sut = new AssignRolesEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var assignedByUserId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var userEvent = new AssignRolesEvent("Admin",
            date,
            assignedByUserId,
            "Test",
            "Test");

        //Act
        var result = _sut.Create(JsonSerializer.Serialize(userEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("User assigned to the following role(s): Admin");
        };

    }
}

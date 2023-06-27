using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.UserManagement.Users.Activities.Factories;
using TaxBeacon.UserManagement.Users.Activities.Models;

namespace TaxBeacon.UserManagement.UnitTests.Services.UserActivities;

public class UserReactivatedEventFactoryTests
{
    private readonly IUserActivityFactory _sut;

    public UserReactivatedEventFactoryTests() => _sut = new UserReactivatedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var reactivatedById = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var userEvent = new UserReactivatedEvent(reactivatedById, date, "Test", "Admin");

        //Act
        var result = _sut.Create(JsonSerializer.Serialize(userEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("User reactivated");
        };

    }
}
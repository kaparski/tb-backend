using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Factories;
using TaxBeacon.Administration.Users.Activities.Models;

namespace TaxBeacon.Administration.UnitTests.Services.Users.Activities;

public class UserDeactivatedEventFactoryTests
{
    private readonly IUserActivityFactory _sut;

    public UserDeactivatedEventFactoryTests() => _sut = new UserDeactivatedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var deactivatedById = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var userEvent = new UserDeactivatedEvent(deactivatedById, date, "Test", "Admin");

        //Act
        var result = _sut.Create(JsonSerializer.Serialize(userEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("User deactivated");
        };

    }
}
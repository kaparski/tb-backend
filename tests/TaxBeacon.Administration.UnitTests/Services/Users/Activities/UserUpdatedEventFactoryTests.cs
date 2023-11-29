using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Administration.Users.Activities.Factories;
using TaxBeacon.Administration.Users.Activities.Models;

namespace TaxBeacon.Administration.UnitTests.Services.Users.Activities;

public class UserUpdatedEventFactoryTests
{
    private readonly IUserActivityFactory _sut;

    public UserUpdatedEventFactoryTests() => _sut = new UserUpdatedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var updatedById = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var userEvent = new UserUpdatedEvent(updatedById, date, "Test", "Admin", "previous", "{\"FirstName\": \"test\", \"LastName\": \"test\"}");

        //Act
        var result = _sut.Create(JsonSerializer.Serialize(userEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("User details updated");
        };

    }
}
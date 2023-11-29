using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Locations.Activities.Factories;
using TaxBeacon.Accounts.Locations.Activities.Models;

namespace TaxBeacon.Accounts.UnitTests.Locations.Activities;

public sealed class LocationCreatedEventFactoryTests
{
    private readonly ILocationActivityFactory _locationActivityFactory = new LocationCreatedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var date = DateTime.UtcNow;
        var userEvent = new LocationCreatedEvent(new Guid(), "Admin", "Test", date);

        //Act
        var result = _locationActivityFactory.Create(JsonSerializer.Serialize(userEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("Location created");
        }
    }
}

using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Locations.Activities.Factories;
using TaxBeacon.Accounts.Locations.Activities.Models;

namespace TaxBeacon.Accounts.UnitTests.Locations.Activities;

public sealed class LocationReactivatedEventFactoryTests
{
    private readonly ILocationActivityFactory _locationActivityFactory = new LocationReactivatedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var date = DateTime.UtcNow;
        var userEvent = new LocationReactivatedEvent(new Guid(), date,"Test", "Admin");

        //Act
        var result = _locationActivityFactory.Create(JsonSerializer.Serialize(userEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("Location reactivated");
        }
    }
}

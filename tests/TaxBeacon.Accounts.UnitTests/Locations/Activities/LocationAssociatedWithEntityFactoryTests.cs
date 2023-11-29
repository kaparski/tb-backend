using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Locations.Activities.Factories;
using TaxBeacon.Accounts.Locations.Activities.Models;

namespace TaxBeacon.Accounts.UnitTests.Locations.Activities;

public sealed class LocationAssociatedWithEntityFactoryTests
{
    private readonly ILocationActivityFactory _locationActivityFactory = new LocationAssociatedWithEntityEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var userEvent = new LocationAssociatedWithEntityEvent(new Guid(), "Admin", "John Testov", date, "Test, Entity 2");

        // Act
        var result = _locationActivityFactory.Create(JsonSerializer.Serialize(userEvent));

        // Assert
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("John Testov");
            result.Message.Should().Be("Location associated with the entity(ies): Test, Entity 2");
        }
    }
}

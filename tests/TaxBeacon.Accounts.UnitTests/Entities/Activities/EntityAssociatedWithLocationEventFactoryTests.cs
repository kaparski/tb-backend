using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Activities.Models;

namespace TaxBeacon.Accounts.UnitTests.Entities.Activities;

public sealed class EntityAssociatedWithLocationEventFactoryTests
{
    private readonly IEntityActivityFactory _entityAssociatedWIthLocationEventFactory = new EntityAssociatedWithLocationEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var testEvent = new EntityAssociatedWithLocationEvent(new Guid(), "Admin", "Test", date, "Test location, location2");

        // Act
        var result = _entityAssociatedWIthLocationEventFactory.Create(JsonSerializer.Serialize(testEvent));

        // Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("Entity associated with the location(s): Test location, location2");
        }
    }
}

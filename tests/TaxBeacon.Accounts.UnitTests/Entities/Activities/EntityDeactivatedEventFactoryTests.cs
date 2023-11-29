using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Activities.Models;

namespace TaxBeacon.Accounts.UnitTests.Entities.Activities;

public sealed class EntityDeactivatedEventFactoryTests
{
    private readonly IEntityActivityFactory _entityDeactivatedEventFactory = new EntityDeactivatedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var entityDeactivated = new EntityDeactivatedEvent(new Guid(), "Admin", "Test", date);

        // Act
        var result = _entityDeactivatedEventFactory.Create(JsonSerializer.Serialize(entityDeactivated));

        // Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("Entity deactivated");
        }
    }
}

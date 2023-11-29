using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Activities.Models;

namespace TaxBeacon.Accounts.UnitTests.Entities.Activities;

public sealed class EntityReactivatedEventFactoryTests
{
    private readonly IEntityActivityFactory _entityReactivatedEventFactory = new EntityReactivatedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var entityReactivated = new EntityReactivatedEvent(new Guid(), "Admin", "Test", date);

        // Act
        var result = _entityReactivatedEventFactory.Create(JsonSerializer.Serialize(entityReactivated));

        // Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("Entity reactivated");
        }
    }

}

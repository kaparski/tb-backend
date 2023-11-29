using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Activities.Models;

namespace TaxBeacon.Accounts.UnitTests.Entities.Activities;

public sealed class EntityCreatedEventFactoryTests
{
    private readonly IEntityActivityFactory _entityCreatedEventFactory = new EntityCreatedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var entityCreated = new EntityCreatedEvent(new Guid(), "Admin", "Test", date);

        // Act
        var result = _entityCreatedEventFactory.Create(JsonSerializer.Serialize(entityCreated));

        // Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("Entity created");
        }
    }
}

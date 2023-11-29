using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Activities.Models;

namespace TaxBeacon.Accounts.UnitTests.Entities.Activities;

public sealed class EntityUpdatedEventFactoryTests
{
    private readonly IEntityActivityFactory _entityUpdatedEventFactory = new EntityUpdatedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var entityUpdated = new EntityUpdatedEvent(new Guid(), "Admin", "Test", date, "Test", "Test-1");

        // Act
        var result = _entityUpdatedEventFactory.Create(JsonSerializer.Serialize(entityUpdated));

        // Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("Entity details updated");
        }
    }
}

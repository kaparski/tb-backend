using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Activities.Models;

namespace TaxBeacon.Accounts.UnitTests.Entities.Activities;

public sealed class StateIdDeletedEventFactoryTests
{
    private readonly IEntityActivityFactory _stateIdAddedEventFactory = new StateIdDeletedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        // Arrange
        const string stateId = "StateId-1";
        var date = DateTime.UtcNow;
        var stateIdAddedEvent = new StateIdDeletedEvent(
            new Guid(),
            "Admin",
            "Test",
            date,
            stateId);

        // Act
        var result = _stateIdAddedEventFactory.Create(JsonSerializer.Serialize(stateIdAddedEvent));

        // Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be($"State ID {stateId} deleted");
        }
    }
}

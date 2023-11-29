using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Activities.Models;

namespace TaxBeacon.Accounts.UnitTests.Entities.Activities;

public sealed class StateIdAddedEventFactoryTests
{
    private readonly IEntityActivityFactory _stateIdAddedEventFactory = new StateIdAddedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        // Arrange
        var stateIds = new List<string> { "StateId-1", "StateId-2" };
        var date = DateTime.UtcNow;
        var stateIdAddedEvent = new StateIdAddedEvent(
            new Guid(),
            "Admin",
            "Test",
            date,
            stateIds);

        // Act
        var result = _stateIdAddedEventFactory.Create(JsonSerializer.Serialize(stateIdAddedEvent));

        // Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be($"State ID(s) {string.Join(", ", stateIds)} added");
        }
    }
}

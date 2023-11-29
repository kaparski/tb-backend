using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Entities.Activities.Factories;
using TaxBeacon.Accounts.Entities.Activities.Models;

namespace TaxBeacon.Accounts.UnitTests.Entities.Activities;

public sealed class StateIdUpdatedEventFactoryTests
{
    private readonly IEntityActivityFactory _stateIdUpdatedEventFactory = new StateIdUpdatedEventFactory();

    [Fact]
    public void Create_ReturnsValidMapping()
    {
        // Arrange
        const string stateId = "StateId-1";
        const string useFullName = "Test";
        var date = DateTime.UtcNow;
        var stateIdUpdatedEvent = new StateIdUpdatedEvent(
            new Guid(),
            "Admin",
            useFullName,
            stateId,
            date);

        // Act
        var result = _stateIdUpdatedEventFactory.Create(JsonSerializer.Serialize(stateIdUpdatedEvent));

        // Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be(useFullName);
            result.Message.Should().Be($"State ID {stateId} updated");
        }
    }
}

using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Factories;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.UnitTests.Contacts.Activities;

public sealed class ContactCreatedEventFactoryTests
{
    private readonly IActivityFactory<ContactEventType> _contactCreatedEventFactory =
        new ContactCreatedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var assignEvent = new ContactCreatedEvent(
            Guid.NewGuid(),
            "Admin",
            "Test",
            date);

        // Act
        var result = _contactCreatedEventFactory.Create(JsonSerializer.Serialize(assignEvent));

        // Assert
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be("Contact created");
        }
    }
}

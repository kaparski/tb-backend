using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Factories;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.UnitTests.Contacts.Activities;

public class ContactLinkedToContactEventFactoryTests
{
    private readonly IActivityFactory<ContactEventType> _contactLinkedToContactEventFactory =
        new ContactLinkedToContactEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var expectedContactName = new Faker().Person.FullName;
        var assignEvent = new ContactLinkedToContactEvent(
            Guid.NewGuid(),
            "Admin",
            "Test",
            date,
            Guid.NewGuid(),
            expectedContactName);

        // Act
        var result = _contactLinkedToContactEventFactory.Create(JsonSerializer.Serialize(assignEvent));

        // Assert
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be($"Contact linked with the contact: {expectedContactName}");
        }
    }
}

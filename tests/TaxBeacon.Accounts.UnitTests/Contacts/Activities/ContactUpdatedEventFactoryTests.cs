using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Factories;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.UnitTests.Contacts.Activities;

public sealed class ContactUpdatedEventFactoryTests
{
    private readonly IActivityFactory<ContactEventType> _contactUpdatedWithAccountEventFactory =
        new ContactUpdatedEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var date = DateTime.UtcNow;
        var contactUpdatedEvent = new ContactUpdatedEvent(
            Guid.NewGuid(),
            "Admin",
            "Some FullName",
            date,
            string.Empty,
            string.Empty);

        //Act
        var result = _contactUpdatedWithAccountEventFactory.Create(JsonSerializer.Serialize(contactUpdatedEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Some FullName");
            result.Message.Should().Be("Contact details updated");
        }
    }
}

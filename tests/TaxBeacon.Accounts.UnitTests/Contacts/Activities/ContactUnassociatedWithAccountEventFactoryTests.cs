using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Factories;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.UnitTests.Contacts.Activities;

public sealed class ContactUnassociatedWithAccountEventFactoryTests
{
    private readonly IActivityFactory<ContactEventType> _contactUnassociatedWithAccountEventFactory =
        new ContactUnassociatedWithAccountEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var date = DateTime.UtcNow;
        const string accountName = "Test account";
        var unassociateEvent = new ContactUnassociatedWithAccountEvent(
            Guid.NewGuid(),
            "Admin",
            "Test",
            date,
            Guid.NewGuid(),
            accountName);

        //Act
        var result = _contactUnassociatedWithAccountEventFactory.Create(JsonSerializer.Serialize(unassociateEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be($"Contact unassociated from the account: {accountName}");
        }
    }
}

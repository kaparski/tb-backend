using FluentAssertions;
using FluentAssertions.Execution;
using System.Text.Json;
using TaxBeacon.Accounts.Contacts.Activities.Factories;
using TaxBeacon.Accounts.Contacts.Activities.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Services;

namespace TaxBeacon.Accounts.UnitTests.Contacts.Activities;

public sealed class ContactAssignedToAccountEventFactoryTests
{
    private readonly IActivityFactory<ContactEventType> _contactAssignedToAccountEventFactory =
        new ContactAssignedToAccountEventFactory();

    [Fact]
    public void Create_CheckMapping()
    {
        //Arrange
        var date = DateTime.UtcNow;
        const string accountName = "Test account";
        var assignEvent = new ContactAssignedToAccountEvent(
            Guid.NewGuid(),
            "Admin",
            "Test",
            date,
            Guid.NewGuid(),
            accountName);

        //Act
        var result = _contactAssignedToAccountEventFactory.Create(JsonSerializer.Serialize(assignEvent));

        //Arrange
        using (new AssertionScope())
        {
            result.Date.Should().Be(date);
            result.FullName.Should().Be("Test");
            result.Message.Should().Be( $"Contact associated with the account: {accountName}");
        }
    }
}

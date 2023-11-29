using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using OneOf.Types;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Errors;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Fact]
    public async Task UnlinkContactFromContactAsync_BothContactsExist_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();

        var contacts = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate(2);

        var linkedContacts = TestData.LinkedContactFaker
            .RuleFor(lc => lc.SourceContactId, _ => contacts[0].Id)
            .RuleFor(lc => lc.RelatedContactId, _ => contacts[1].Id)
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate(2);
        linkedContacts[1].RelatedContactId = linkedContacts[0].SourceContactId;
        linkedContacts[1].SourceContactId = linkedContacts[0].RelatedContactId;

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Contacts.AddRangeAsync(contacts);
        await _dbContext.LinkedContacts.AddRangeAsync(linkedContacts);

        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService
            .UnlinkContactFromContactAsync(linkedContacts[0].SourceContactId, linkedContacts[0].RelatedContactId);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);

            actualResult.TryPickT0(out var success, out _).Should().BeTrue();
            success.Should().BeOfType<Success>();

            var actualAccountContact = await _dbContext.LinkedContacts.ToListAsync();
            actualAccountContact.Should().HaveCount(0);

            var actualActivityForContact1 = await _dbContext.ContactActivityLogs.SingleOrDefaultAsync(l => l.ContactId == contacts[0].Id);
            actualActivityForContact1.Should().NotBeNull();
            actualActivityForContact1?.TenantId.Should().Be(tenant.Id);
            actualActivityForContact1?.ContactId.Should().Be(contacts[0].Id);
            actualActivityForContact1?.EventType.Should().Be(ContactEventType.ContactUnlinkedFromContact);

            var actualActivityForContact2 = await _dbContext.ContactActivityLogs.SingleOrDefaultAsync(l => l.ContactId == contacts[1].Id);
            actualActivityForContact2.Should().NotBeNull();
            actualActivityForContact2?.TenantId.Should().Be(tenant.Id);
            actualActivityForContact2?.ContactId.Should().Be(contacts[1].Id);
            actualActivityForContact2?.EventType.Should().Be(ContactEventType.ContactUnlinkedFromContact);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(4));
        }
    }

    [Fact]
    public async Task UnlinkContactFromContactAsync_ContactDoesNotExistInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();

        var contacts = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate(2);

        var linkedContacts = TestData.LinkedContactFaker
            .RuleFor(lc => lc.SourceContactId, _ => contacts[0].Id)
            .RuleFor(lc => lc.RelatedContactId, _ => contacts[1].Id)
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate(2);
        linkedContacts[1].RelatedContactId = linkedContacts[0].SourceContactId;
        linkedContacts[1].SourceContactId = linkedContacts[0].RelatedContactId;

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Contacts.AddRangeAsync(contacts);
        await _dbContext.LinkedContacts.AddRangeAsync(linkedContacts);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService
            .UnlinkContactFromContactAsync(contacts[0].Id, Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}

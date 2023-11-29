using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;
using Moq;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Fact]
    public async Task AssignContactToAccountAsync_AccountAndContactExistInDb_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        var contact = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Contacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        var expectedResult = new AccountContact
        {
            AccountId = account.Id,
            Account = account,
            ContactId = contact.Id,
            Contact = contact,
            TenantId = tenant.Id,
            Tenant = tenant,
            Status = Status.Active,
            Type = ContactType.Client.Name
        };

        // Act
        var actualResult = await _contactService
            .AssignContactToAccountAsync(account.Id, contact.Id, ContactType.Client);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);

            actualResult.TryPickT0(out var success, out _).Should().BeTrue();
            success.Should().BeOfType<Success>();

            var actualAccountContact = await _dbContext.AccountContacts.LastOrDefaultAsync();
            actualAccountContact.Should().NotBeNull();
            actualAccountContact.Should().BeEquivalentTo(expectedResult);

            var actualActivity = await _dbContext.ContactActivityLogs.LastOrDefaultAsync();
            actualActivity.Should().NotBeNull();
            actualActivity?.TenantId.Should().Be(tenant.Id);
            actualActivity?.ContactId.Should().Be(contact.Id);
            actualActivity?.EventType.Should().Be(ContactEventType.ContactAssignedToAccount);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(3));
        }
    }

    [Fact]
    public async Task AssignContactToAccountAsync_AccountDoesNotExistInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        var contact = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Contacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService
            .AssignContactToAccountAsync(Guid.NewGuid(), contact.Id, ContactType.Client);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }

    [Fact]
    public async Task AssignContactToAccountAsync_ContactDoesNotExistInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        var contact = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Contacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService
            .AssignContactToAccountAsync(account.Id, Guid.NewGuid(), ContactType.Client);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}

using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using OneOf.Types;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Errors;
using TaxBeacon.DAL.Accounts.Entities;
using Moq;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Fact]
    public async Task UnassociateContactToAccountAsync_AccountAndContactExistInDb_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var accounts = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate(2);

        var contact = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        _dbContext.Accounts.AddRange(accounts);
        await _dbContext.Contacts.AddAsync(contact);
        var links = new List<AccountContact>
        {
            new AccountContact
            {
                AccountId = accounts[0].Id,
                ContactId = contact.Id,
                Status = Status.Active,
                TenantId = tenant.Id,
                Type = "Employee",
            },
            new AccountContact
            {
                AccountId = accounts[1].Id,
                ContactId = contact.Id,
                Status = Status.Active,
                TenantId = tenant.Id,
                Type = "Employee2",
            }
        };
        _dbContext.AccountContacts.AddRange(links);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService
            .UnassociateContactWithAccount(accounts[0].Id, contact.Id, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);

            actualResult.TryPickT0(out var success, out _).Should().BeTrue();
            success.Should().BeOfType<Success>();

            var actualAccountContact = await _dbContext.AccountContacts.ToListAsync();
            actualAccountContact.Should().HaveCount(1).And.ContainEquivalentOf(links[1]);

            var actualActivity = await _dbContext.ContactActivityLogs.LastOrDefaultAsync();
            actualActivity.Should().NotBeNull();
            actualActivity?.TenantId.Should().Be(tenant.Id);
            actualActivity?.ContactId.Should().Be(contact.Id);
            actualActivity?.EventType.Should().Be(ContactEventType.ContactUnassociatedWithAccount);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(2));
        }
    }

    [Fact]
    public async Task UnassociateContactToAccountAsync_AccountDoesNotExistInDb_ReturnsNotFound()
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
            .UnassociateContactWithAccount(Guid.NewGuid(), contact.Id, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }

    [Fact]
    public async Task UnassociateContactToAccountAsync_ContactDoesNotExistInDb_ReturnsNotFound()
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

    [Fact]
    public async Task UnassociateContactToAccountAsync_ContactIsAssociatedWithOnlyOneAccount_ReturnsInvalidOperation()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var accounts = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        var contact = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        _dbContext.Accounts.AddRange(accounts);
        await _dbContext.Contacts.AddAsync(contact);
        var links = new List<AccountContact>
        {
            new AccountContact
            {
                AccountId = accounts.Id,
                ContactId = contact.Id,
                Status = Status.Active,
                TenantId = tenant.Id,
                Type = "Employee",
            },
        };
        _dbContext.AccountContacts.AddRange(links);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService
            .UnassociateContactWithAccount(accounts.Id, contact.Id, CancellationToken.None);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT2(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<InvalidOperation>();
        }
    }
}

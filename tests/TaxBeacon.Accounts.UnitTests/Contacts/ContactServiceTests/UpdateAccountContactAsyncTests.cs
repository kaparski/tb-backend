// ReSharper disable once CheckNamespace

using FluentAssertions;
using FluentAssertions.Execution;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Contacts;

public partial class ContactServiceTests
{

    [Fact]
    public async Task UpdateAccountContactAsync_AccountDoesNotExistInDb_ReturnsNotFound()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService.UpdateAccountContactAsync(Guid.NewGuid(), Guid.NewGuid(), new UpdateAccountContactDto());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task UpdateAccountContactAsync_UpdateAccountContactSuccessfully_ReturnsAccountContactDto()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var contact = TestData.ContactFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        contact.Accounts = new List<AccountContact>
        {
            new() { Account = account, Type = ContactType.Client.Name, Status = Status.Active }
        };

        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.Contacts.AddAsync(contact);
        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.SaveChangesAsync();

        var updateAccountContactDto = TestData.UpdateAccountContactFaker
            .RuleFor(x => x.Type, _ => ContactType.ReferralPartner)
            .Generate();
        var expectedAccountContact = updateAccountContactDto.Adapt<AccountContact>();
        expectedAccountContact.TenantId = tenant.Id;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);
        var utcNow = DateTime.UtcNow;
        var utcNow2 = utcNow.AddMilliseconds(1);
        _dateTimeServiceMock
            .SetupSequence(ds => ds.UtcNow)
            .Returns(utcNow)
            .Returns(utcNow2);

        var expectedContactActivityLog = new ContactActivityLog()
        {
            TenantId = tenant.Id,
            ContactId = contact.Id,
            EventType = ContactEventType.ContactUpdated,
            Date = utcNow,
            Revision = 1
        };

        var expectedAccountContactActivityLog = new AccountContactActivityLog()
        {
            TenantId = tenant.Id,
            ContactId = contact.Id,
            AccountId = account.Id,
            EventType = ContactEventType.ContactTypeUpdated,
            Date = utcNow2,
            Revision = 1
        };

        // Act
        var actualResult = await _contactService.UpdateAccountContactAsync(contact.Id, account.Id, updateAccountContactDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var createdAccountContact, out _).Should().BeTrue();
            (await _dbContext.SaveChangesAsync()).Should().Be(0);
            createdAccountContact.Should().BeEquivalentTo(expectedAccountContact, opt =>
            {
                opt.Excluding(x => x.Status);
                opt.ExcludingMissingMembers();
                return opt;
            });
            createdAccountContact.Should().BeEquivalentTo(expectedAccountContact.Contact, opt =>
            {
                opt.Excluding(x => x.Id);
                opt.Excluding(x => x.CreatedDateTimeUtc);
                opt.Excluding(x => x.Phones);
                opt.Excluding(x => x.LinkedContacts);
                opt.Excluding(x => x.Accounts);
                opt.ExcludingMissingMembers();
                return opt;
            });
            createdAccountContact.Phones.Should().BeEquivalentTo(updateAccountContactDto.Phones, opt => opt.ExcludingMissingMembers());

            var contactActivity = await _dbContext.ContactActivityLogs.LastOrDefaultAsync();
            var accountContactActivity = await _dbContext.AccountContactActivityLogs.LastOrDefaultAsync();
            var contactCount = await _dbContext.ContactActivityLogs.CountAsync();
            var accountContactCount = await _dbContext.AccountContactActivityLogs.CountAsync();
            accountContactCount.Should().Be(1);
            contactCount.Should().Be(1);
            contactActivity.Should().BeEquivalentTo(expectedContactActivityLog, opt =>
            {
                opt.WithStrictOrdering();
                opt.Excluding(x => x.Event);
                opt.Excluding(x => x.Contact);
                opt.Excluding(x => x.Tenant);
                opt.ExcludingMissingMembers();
                return opt;
            });

            accountContactActivity.Should().BeEquivalentTo(expectedAccountContactActivityLog, opt =>
            {
                opt.WithStrictOrdering();
                opt.Excluding(x => x.Event);
                opt.Excluding(x => x.AccountContact);
                opt.Excluding(x => x.Tenant);
                opt.ExcludingMissingMembers();
                return opt;
            });

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(2));
        }
    }
}

using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using OneOf.Types;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Fact]
    public async Task GetAccountContactDetailsAsync_ContactExists_ReturnsContact()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .RuleFor(c => c.Accounts,
                _ => new List<AccountContact>
                {
                    new() { Account = account, Type = ContactType.Client.Name, Status = Status.Active }
                })
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.Contacts.AddRangeAsync(contact);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        var expectedResult = await _dbContext.AccountContacts.ProjectToType<AccountContactDto>().FirstAsync();

        // Act
        var getContactResult = await _contactService.GetAccountContactDetailsAsync(account.Id, contact.Id);

        // Assert
        using (new AssertionScope())
        {
            getContactResult.TryPickT0(out var contactDto, out _).Should().BeTrue();
            contactDto.Should().BeEquivalentTo(expectedResult, opt => opt.ExcludingMissingMembers());
        }
    }

    [Fact]
    public async Task GetAccountContactDetailsAsync_ContactDoesntBelongToAccount_ReturnsNotFound()
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
        await _dbContext.Contacts.AddRangeAsync(contact);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        // Act
        var getContactResult = await _contactService.GetAccountContactDetailsAsync(account.Id, contact.Id);

        // Assert
        using (new AssertionScope())
        {
            getContactResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}

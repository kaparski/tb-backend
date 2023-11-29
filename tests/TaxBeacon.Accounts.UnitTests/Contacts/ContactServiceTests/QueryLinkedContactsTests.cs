using FluentAssertions.Execution;
using FluentAssertions;
using OneOf.Types;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Fact]
    public async Task QueryLinkedContacts_ContactExists_ReturnsContacts()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contacts = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .RuleFor(c => c.Accounts,
                _ => new List<AccountContact>
                {
                    new() { Account = account, Type = ContactType.Client.Name, Status = Status.Active }
                })
            .Generate(2);

        var linkedContact = TestData.LinkedContactFaker
            .RuleFor(c => c.SourceContactId, _ => contacts[0].Id)
            .RuleFor(c => c.RelatedContactId, _ => contacts[1].Id)
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.Contacts.AddRangeAsync(contacts);
        await _dbContext.LinkedContacts.AddAsync(linkedContact);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        var expectedResult = new List<LinkedContactDetailsDto>
        {
            new()
            {
            Id = linkedContact.RelatedContactId,
            SourceContactId = linkedContact.SourceContactId,
            Comment = linkedContact.Comment,
            Email = contacts[1].Email,
            Name = contacts[1].FullName,
            FirstName = contacts[1].FirstName,
            LastName = contacts[1].LastName,
            AccountNames = new List<string>() { account.Name }
            }
        };

        // Act
        var getLinkedContactsResult = await _contactService.QueryLinkedContacts(contacts[0].Id);

        // Assert
        using (new AssertionScope())
        {
            getLinkedContactsResult.TryPickT0(out var linkedContactDetails, out _).Should().BeTrue();
            linkedContactDetails.Should().BeAssignableTo<IQueryable<LinkedContactDetailsDto>>();
            linkedContactDetails.Should().HaveCount(1);
            linkedContactDetails.Should().BeEquivalentTo(expectedResult, opt => opt.ExcludingMissingMembers());
        }
    }

    [Fact]
    public async Task QueryLinkedContacts_ContactDoesntExist_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contacts = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate(2);

        var linkedContact = TestData.LinkedContactFaker
            .RuleFor(c => c.SourceContactId, _ => contacts[0].Id)
            .RuleFor(c => c.RelatedContactId, _ => contacts[1].Id)
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Contacts.AddRangeAsync(contacts);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.LinkedContacts.AddAsync(linkedContact);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        // Act
        var getContactResult = await _contactService.QueryLinkedContacts(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            getContactResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}

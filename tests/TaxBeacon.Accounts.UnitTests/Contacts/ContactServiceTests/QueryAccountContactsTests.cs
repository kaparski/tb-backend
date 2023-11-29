using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Fact]
    public async Task QueryAccountContacts_AccountsExists_ReturnsContacts()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.Id, _ => Guid.NewGuid())
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        var contacts = TestData.ContactFaker
            .RuleFor(a => a.Id, _ => Guid.NewGuid())
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .RuleFor(c => c.Accounts,
                _ => new List<AccountContact>
                {
                    new() { Account = account, Type = ContactType.Client.Name, Status = Status.Active }
                })
            .Generate(3);

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.Contacts.AddRangeAsync(contacts);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        var expectedContacts = await _dbContext.AccountContacts
            .ProjectToType<AccountContactDto>()
            .ToListAsync();

        // Act
        var result = _contactService.QueryAccountContacts(account.Id);

        // Assert
        using (new AssertionScope())
        {
            result.Should().HaveCount(3);
            result.Should().BeEquivalentTo(expectedContacts);
        }
    }
}

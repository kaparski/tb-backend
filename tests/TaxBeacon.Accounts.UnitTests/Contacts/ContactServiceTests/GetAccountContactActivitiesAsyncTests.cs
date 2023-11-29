using FluentAssertions.Execution;
using FluentAssertions;
using OneOf.Types;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Fact]
    public async Task GetAccountContactActivitiesAsync_ContactExistsInDb_ReturnsActivities()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate();
        var contactActivityLogs = TestData.ContactActivityFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.ContactId, _ => contact.Id)
            .Generate(2);
        var accountContactActivityLogs = TestData.AccountContactActivityFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.ContactId, _ => contact.Id)
            .Generate(2);
        contact.Accounts = new List<AccountContact>
        {
            new() { Account = account, Type = ContactType.Client.Name, Status = Status.Active }
        };

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.Contacts.AddAsync(contact);
        await _dbContext.ContactActivityLogs.AddRangeAsync(contactActivityLogs);
        await _dbContext.AccountContactActivityLogs.AddRangeAsync(accountContactActivityLogs);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService.GetAccountContactActivitiesAsync(account.Id, contact.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activity, out _).Should().BeTrue();
            activity.Query.Count().Should().Be(contactActivityLogs.Count + accountContactActivityLogs.Count);
        }
    }

    [Fact]
    public async Task GetAccountContactActivitiesAsync_ContactDoesNotExistInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContext.Tenants.AddAsync(tenant);
        await _dbContext.Accounts.AddAsync(account);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService.GetAccountContactActivitiesAsync(account.Id, Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }

    [Fact]
    public async Task GetAccountContactActivitiesAsync_ContactDoesntAssignedToAccount_ReturnsActivities()
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
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService.GetAccountContactActivitiesAsync(account.Id, contact.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}

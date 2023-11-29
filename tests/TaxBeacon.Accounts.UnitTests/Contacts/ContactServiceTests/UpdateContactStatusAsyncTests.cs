using FluentAssertions.Execution;
using FluentAssertions;
using OneOf.Types;
using Moq;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Fact]
    public async Task UpdateContactStatusAsync_ActiveStatusAndContactAssignedToAccount_ReturnsUpdatedContact()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate();

        contact.Accounts = new List<AccountContact>
        {
            new() { Account = account, Type = ContactType.Client.Name, Status = Status.Deactivated }
        };

        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.Contacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .SetupGet(ds => ds.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _contactService.UpdateAccountContactStatusAsync(account.Id, contact.Id, Status.Active);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var dto, out _).Should().BeTrue();
            dto.Status.Should().Be(Status.Active);
            dto.DeactivationDateTimeUtc.Should().BeNull();
            dto.ReactivationDateTimeUtc.Should().Be(currentDate);
            dto.Should().BeEquivalentTo(contact, opt => opt.ExcludingMissingMembers());

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(3));
        }
    }

    [Fact]
    public async Task UpdateContactStatusAsync_DeactivatedStatusAndContactAssignedToAccount_ReturnsUpdatedContact()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .Generate();

        contact.Accounts = new List<AccountContact>
        {
            new() { Account = account, Type = ContactType.Client.Name, Status = Status.Active }
        };

        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.Contacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .SetupGet(ds => ds.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);
        //Act
        var actualResult =
            await _contactService.UpdateAccountContactStatusAsync(account.Id, contact.Id, Status.Deactivated);

        //Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var dto, out _).Should().BeTrue();
            dto.Status.Should().Be(Status.Deactivated);
            dto.ReactivationDateTimeUtc.Should().BeNull();
            dto.DeactivationDateTimeUtc.Should().Be(currentDate);
            dto.Should().BeEquivalentTo(contact, opt => opt.ExcludingMissingMembers());

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(3));
        }
    }

    [Fact]
    public async Task UpdateContactStatusAsync_ContactDoesntBelongToAccount_ReturnsNotFound()
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
        var actualResult =
            await _contactService.UpdateAccountContactStatusAsync(account.Id, Guid.NewGuid(), Status.Deactivated);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }
}

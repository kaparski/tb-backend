using TaxBeacon.Common.Enums.Accounts;
using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Fact]
    public async Task GetAccountDetailsByIdAsync_AccountExists_ReturnsAccountDetailsDto()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var accounts = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .RuleFor(a => a.Phones, _ => TestData.PhoneFaker.Generate(3))
            .Generate(3);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddRangeAsync(accounts);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _accountService.GetAccountDetailsByIdAsync(accounts[0].Id, AccountInfoType.Full);

        // Assert
        actualResult.TryPickT0(out var actualAccount, out _).Should().BeTrue();
        actualAccount.Should().BeEquivalentTo(accounts[0], opt => opt.ExcludingMissingMembers());
        actualAccount.Phones.Should().BeEquivalentTo(accounts[0].Phones, opt => opt.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetAccountDetailsByIdAsync_NonExistingTenantId_ReturnsNotFound()
    {
        // Act
        var actualResult = await _accountService.GetAccountDetailsByIdAsync(Guid.NewGuid(), AccountInfoType.Full);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Fact]
    public async Task GetAccountDetailsByIdAsync_AccountTenantIdNotEqualCurrentUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var accounts = TestData.AccountsViewFaker
            .RuleFor(a => a.TenantId, _ => tenants[^1].Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.AccountsView.AddRangeAsync(accounts);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[0].Id);

        // Act
        var actualResult = await _accountService.GetAccountDetailsByIdAsync(accounts[0].Id, AccountInfoType.Full);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }
}

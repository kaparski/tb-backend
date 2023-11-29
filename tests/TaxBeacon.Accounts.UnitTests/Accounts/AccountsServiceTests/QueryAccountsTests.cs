using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Fact]
    public async Task QueryAccounts_ReturnsAccountsDto()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var accounts = TestData.AccountsViewFaker
            .RuleFor(a => a.TenantId, f => f.PickRandom(tenants.Select(t => t.Id)))
            .Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.AccountsView.AddRangeAsync(accounts);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[0].Id);

        var expectedAccounts = accounts
            .Where(a => a.TenantId == tenants[0].Id)
            .ToList();

        // Act
        var actualResult = await _accountService.QueryAccounts().ToListAsync();

        // Assert

        using (new AssertionScope())
        {
            actualResult.Should().HaveCount(expectedAccounts.Count)
                .And.AllBeOfType<AccountDto>()
                .And.BeEquivalentTo(expectedAccounts, opt => opt.ExcludingMissingMembers());
        }
    }

    [Fact]
    public async Task QueryAccounts_ReturnsAccountManagersField()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var accountsView = TestData.AccountsViewFaker
            .RuleFor(a => a.TenantId, tenant.Id)
            .Generate(5);
        var accounts = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate(5);

        for (var i = 0; i < 5; i++)
        {
            accountsView[i].Id = accounts[i].Id;
        }

        var users = TestData.UserFaker
            .Generate(8)
            .Select(x => new TenantUser
            {
                TenantId = tenant.Id,
                UserId = x.Id,
                User = x
            })
        .ToList();

        var managerIds = new List<(Guid accountId, Guid userId)>();

        for (var i = 0; i < accounts.Count; i++)
        {
            var accountId = accounts[i].Id;
            accounts[i].Client = TestData.ClientsFaker
                .RuleFor(c => c.TenantId, _ => tenant.Id)
                .RuleFor(c => c.ClientManagers, f => f.PickRandom(users, 3).Select(u =>
                {
                    var client = new ClientManager { TenantId = tenant.Id, TenantUser = u };
                    managerIds.Add((accountId, u.UserId));
                    return client;
                }).ToList())
                .Generate();

            accountsView[i].Id = accounts[i].Client!.AccountId = accounts[i].Id = accountId;
            accountsView[i].ClientAccountManagers = string.Join(", ", accounts[i].Client!.ClientManagers.Select(x => x.TenantUser.User.FullName));
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TenantUsers.AddRangeAsync(users);
        await _dbContextMock.Accounts.AddRangeAsync(accounts);
        await _dbContextMock.AccountsView.AddRangeAsync(accountsView);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var expectedAccounts = accountsView
            .ToList();

        var expectedManagersIds = managerIds.ToLookup(x => x.accountId);

        // Act
        var actualResult = await _accountService.QueryAccounts().ToListAsync();

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveCount(expectedAccounts.Count)
                .And.AllBeOfType<AccountDto>();

            actualResult.Should().BeEquivalentTo(accounts.Select(a => new
            {
                Id = a.Id,
                ClientAccountManagerIds = expectedManagersIds[a.Id].Select(x => x.userId),
            }));
        }
    }
}

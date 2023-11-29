using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Fact]
    public async Task QueryReferralPartners_ReturnsReferralPartnerDto()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        await _dbContextMock.Tenants.AddRangeAsync(tenants);

        var tenantId = tenants[0].Id;
        var referrals = TestData.ReferralsViewFaker
            .RuleFor(a => a.TenantId, f => tenants[0].Id)
            .RuleFor(a => a.ReferralState, f => ReferralState.ReferralPartner.Name)
            .Generate(3);
        await _dbContextMock.ReferralsView.AddRangeAsync(referrals);
        await _dbContextMock.SaveChangesAsync();
        var expectedReferrals = referrals;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[0].Id);

        // Act
        var actualResult = await _accountService
            .QueryReferralPartners()
            .ToListAsync();

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveCount(expectedReferrals.Count)
                .And.AllBeOfType<ReferralPartnerDto>();
        }
    }

    [Fact]
    public async Task QueryReferralPartners_ReturnsSalespersonsField()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();

        var users = TestData.UserFaker
            .Generate(8)
            .Select(x => new TenantUser
            {
                TenantId = tenant.Id,
                UserId = x.Id,
                User = x
            })
            .ToList();

        var accounts = TestData.AccountsFaker
            .RuleFor(x => x.TenantId, tenant.Id)
            .RuleFor(c => c.Salespersons, f => f.PickRandom(users, 3).Select(u =>
                new AccountSalesperson { TenantId = tenant.Id, TenantUser = u }).ToList())
            .Generate(5);

        var referralsView = TestData.ReferralsViewFaker
            .RuleFor(x => x.TenantId, tenant.Id)
            .RuleFor(x => x.AccountManagers, f => string.Join(" ,", f.PickRandom(users.Select(x => x.User.FullName), 3)))
            .RuleFor(a => a.ReferralState, f => ReferralState.ReferralPartner.Name)
            .Generate(5);

        var referrals = TestData.ReferralsFaker
                .RuleFor(c => c.TenantId, _ => tenant.Id)
                .RuleFor(a => a.State, f => ReferralState.ReferralPartner.Name)
                .Generate(5);

        for (var i = 0; i < referrals.Count; i++)
        {
            referrals[i].Account = accounts[i];
            referralsView[i].AccountId = referrals[i].AccountId = accounts[i].Id;
            referralsView[i].AccountManagers = string.Join(", ", referrals[i].ReferralManagers.Select(x => x.TenantUser.User.FullName));
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TenantUsers.AddRangeAsync(users);
        await _dbContextMock.Accounts.AddRangeAsync(accounts);
        await _dbContextMock.Referrals.AddRangeAsync(referrals);
        await _dbContextMock.ReferralsView.AddRangeAsync(referralsView);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _accountService.QueryReferralPartners().ToListAsync();

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveCount(referralsView.Count)
                .And.AllBeOfType<ReferralPartnerDto>();

            actualResult.Should().BeEquivalentTo(referralsView.Select(r => new
            {
                Id = r.AccountId,
                r.Salespersons,
                SalespersonIds = accounts.First(x => x.Id == r.AccountId).Salespersons.Select(x => x.UserId),
            }));
        }
    }
}

using FluentAssertions;
using Org.BouncyCastle.Ocsp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public partial class AccountsServiceTests
{
    [Fact]
    public async Task GenerateUniqueLocationIdAsync_LocationDoesNotExistInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var idIncrement = 100_000_000;
        var accounts = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, () => idIncrement++.ToString())
            .Generate(10000);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddRangeAsync(accounts);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);
        var locationIds = accounts.Select(x => x.AccountId).ToHashSet();
        for (var i = 0; i < 100; i++)
        {
            var actualResult = await _accountService.GenerateUniqueAccountIdAsync(CancellationToken.None);

            actualResult.IsT0.Should().BeTrue();
            locationIds.Should().NotContain(actualResult.AsT0);
        }
    }
}

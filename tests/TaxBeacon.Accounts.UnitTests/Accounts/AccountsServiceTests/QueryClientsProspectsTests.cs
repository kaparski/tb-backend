using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Fact]
    public async Task QueryClientsProspects_ReturnsClientProspectDto()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        await _dbContextMock.Tenants.AddRangeAsync(tenants);

        var tenantId = tenants[0].Id;
        var clients = TestData.ClientsViewFaker
            .RuleFor(a => a.TenantId, f => tenants[0].Id)
            .RuleFor(a => a.ClientState, f => "Client prospect")
            .Generate(3);
        await _dbContextMock.ClientsView.AddRangeAsync(clients);
        await _dbContextMock.SaveChangesAsync();
        var expectedClients = clients;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[0].Id);

        // Act
        var actualResult = await _accountService
            .QueryClientsProspects()
            .ToListAsync();

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveCount(expectedClients.Count)
                .And.AllBeOfType<ClientProspectDto>();
        }
    }

    [Fact]
    public async Task QueryClientsProspects_ReturnsSalespersonsField()
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

        var clientsView = TestData.ClientsViewFaker
            .RuleFor(x => x.TenantId, tenant.Id)
            .RuleFor(x => x.AccountManagers, f => string.Join(" ,", f.PickRandom(users.Select(x => x.User.FullName))))
            .RuleFor(a => a.ClientState, f => ClientState.ClientProspect.Name)
            .Generate(5);

        var clients = TestData.ClientsFaker
                .RuleFor(c => c.TenantId, _ => tenant.Id)
                .RuleFor(a => a.State, f => ClientState.ClientProspect.Name)
                .Generate(5);

        for (var i = 0; i < clients.Count; i++)
        {
            clients[i].Account = accounts[i];
            clientsView[i].AccountId = clients[i].AccountId = accounts[i].Id;
            clientsView[i].AccountManagers = string.Join(", ", clients[i].ClientManagers.Select(x => x.TenantUser.User.FullName));
        }

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TenantUsers.AddRangeAsync(users);
        await _dbContextMock.Accounts.AddRangeAsync(accounts);
        await _dbContextMock.Clients.AddRangeAsync(clients);
        await _dbContextMock.ClientsView.AddRangeAsync(clientsView);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _accountService.QueryClientsProspects().ToListAsync();

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveCount(clientsView.Count)
                .And.AllBeOfType<ClientProspectDto>();

            actualResult.Should().BeEquivalentTo(clientsView.Select(c => new
            {
                AccountId = c.AccountId,
                Salespersons = c.Salespersons,
                SalespersonIds = accounts.First(x => x.Id == c.AccountId).Salespersons.Select(x => x.UserId),
            }));
        }
    }
}

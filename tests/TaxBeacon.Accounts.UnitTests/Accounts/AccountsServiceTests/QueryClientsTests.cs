using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Fact]
    public async Task QueryClients_ReturnsClientsDto()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        await _dbContextMock.Tenants.AddRangeAsync(tenants);

        var tenantId = tenants[0].Id;
        var clients = TestData.ClientsViewFaker
            .RuleFor(a => a.TenantId, f => tenants[0].Id)
            .RuleFor(a => a.ClientState, f => ClientState.Client.Name)
            .Generate(3);
        await _dbContextMock.ClientsView.AddRangeAsync(clients);
        await _dbContextMock.SaveChangesAsync();
        var expectedClients = clients;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[0].Id);

        // Act
        var actualResult = await _accountService
            .QueryClients()
            .ToListAsync();

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveCount(expectedClients.Count)
                .And.AllBeOfType<ClientDto>();
        }
    }

    [Fact]
    public async Task QueryClients__ReturnsAccountManagersField()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();

        var accounts = TestData.AccountsFaker
            .RuleFor(x => x.TenantId, tenant.Id)
            .Generate(5);

        var users = TestData.UserFaker
            .Generate(8)
            .Select(x => new TenantUser
            {
                TenantId = tenant.Id,
                UserId = x.Id,
                User = x
            })
            .ToList();

        var clientsView = TestData.ClientsViewFaker
            .RuleFor(x => x.TenantId, tenant.Id)
            .RuleFor(x => x.AccountManagers, f => string.Join(" ,", f.PickRandom(users.Select(x => x.User.FullName))))
            .RuleFor(a => a.ClientState, f => ClientState.Client.Name)
            .Generate(5);

        var clients = TestData.ClientsFaker
                .RuleFor(c => c.TenantId, _ => tenant.Id)
                .RuleFor(a => a.State, f => ClientState.Client.Name)
                .RuleFor(c => c.ClientManagers, f => f.PickRandom(users, 3).Select(u =>
                new ClientManager { TenantId = tenant.Id, TenantUser = u }).ToList())
                .Generate(5);

        for (var i = 0; i < clients.Count; i++)
        {
            clients[i].Account = accounts[i];
            clientsView[i].AccountId = clients[i].AccountId = accounts[i].Id;
            clientsView[i].AccountIdField = accounts[i].AccountId;
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
        var actualResult = await _accountService.QueryClients().ToListAsync();

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveCount(clientsView.Count)
                .And.AllBeOfType<ClientDto>();

            actualResult.Should().BeEquivalentTo(clientsView.Select(c => new
            {
                Id = c.AccountId,
                AccountManagers = c.AccountManagers,
                AccountManagerIds = clients.First(x => x.AccountId == c.AccountId).ClientManagers.Select(x => x.UserId),

            }));
        }
    }
}

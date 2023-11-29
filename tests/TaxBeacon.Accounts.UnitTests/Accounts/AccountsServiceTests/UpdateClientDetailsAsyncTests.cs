using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using Mapster;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Fact]
    public async Task UpdateClientDetailsAsync_ClientDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateClientDto = TestData.UpdateClientDtoFaker.Generate();
        var client = TestData.ClientsFaker.Generate();

        // Act
        var actualResult = await _accountService.UpdateClientDetailsAsync(client.Account.Id, updateClientDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task UpdateClientDetailsAsync_UpdateClientDataWithoutAccountManagers_ReturnsAccountDetailsDtoAndCapturesActivityLog()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var client = TestData.ClientsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .RuleFor(a => a.Account, _ => account)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Clients.AddAsync(client);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        _userServiceMock
            .Setup(x => x.UserExistsInTenantAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(true);

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        var request = TestData.UpdateClientDtoFaker.Generate();

        // Act
        var actualResult =
            await _accountService.UpdateClientDetailsAsync(client.AccountId, request);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var accountDetailsDto, out _);
            accountDetailsDto.Should().NotBeNull();
            accountDetailsDto.Should().BeEquivalentTo(request, opt => opt.ExcludingMissingMembers());

            var actualActivityLog = await _dbContextMock.AccountActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(AccountEventType.ClientDetailsUpdated);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);
            actualActivityLog?.AccountId.Should().Be(client.AccountId);
            actualActivityLog?.AccountPartType.Should().Be(AccountPartActivityType.Client);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateClientDetailsAsync_UpdateClientDetailsWithAccountManagers_ReturnsAccountDetailsDtoAndCapturesActivityLog()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var users = TestData.UserFaker.Generate(5);
        var tenantUsers = users
            .Select(u => new TenantUser { UserId = u.Id, TenantId = tenant.Id })
            .ToList();
        var client = TestData.ClientsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .RuleFor(a => a.Account, _ => account)
            .RuleFor(a => a.ClientManagers, _ =>
                tenantUsers.Select(tu => new ClientManager
                {
                    TenantId = tu.TenantId,
                    UserId = tu.UserId
                }).Take(3).ToList())
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TenantUsers.AddRangeAsync(tenantUsers);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Clients.AddAsync(client);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        _userServiceMock
            .Setup(x => x.UserExistsInTenantAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(true);
        var clientDetailsUpdatedDate = DateTime.UtcNow;
        var accountManagersUnassignedDate = clientDetailsUpdatedDate.AddMilliseconds(1);
        var accountManagersAssignedDate = clientDetailsUpdatedDate.AddMilliseconds(2);
        _dateTimeServiceMock
            .SetupSequence(service => service.UtcNow)
            .Returns(clientDetailsUpdatedDate)
            .Returns(accountManagersUnassignedDate)
            .Returns(accountManagersAssignedDate);

        var expectedActivityLogs = new List<AccountActivityLog>()
            {
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.ClientAccountManagerAssigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.Client,
                    Date = accountManagersAssignedDate
                },
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.ClientAccountManagerUnassigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.Client,
                    Date = accountManagersUnassignedDate
                },
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.ClientDetailsUpdated,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.Client,
                    Date = clientDetailsUpdatedDate
                }
            };

        // new account managers ids contain 2 new ids and one existing id
        var newClientManagersIds = tenantUsers.Select(tu => tu.UserId).TakeLast(3).ToList();

        var request = TestData.UpdateClientDtoFaker
            .RuleFor(x => x.ClientManagersIds, _ => newClientManagersIds)
            .Generate();

        // Act
        var actualResult =
            await _accountService.UpdateClientDetailsAsync(client.AccountId, request);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var accountDetailsDto, out _);
            accountDetailsDto.Should().NotBeNull();
            accountDetailsDto.Should().BeEquivalentTo(request, opt => opt.ExcludingMissingMembers());
            var accountManagersIds = accountDetailsDto.Client!.ClientManagers.Select(sp => sp.UserId);
            accountManagersIds.Should().BeEquivalentTo(newClientManagersIds);

            var actualActivityLogs = await _dbContextMock.AccountActivityLogs.OrderByDescending(a => a.Date).ToListAsync();
            actualActivityLogs.Should().NotBeNull();
            actualActivityLogs.Should().BeEquivalentTo(expectedActivityLogs, opt =>
            {
                opt.WithStrictOrdering();
                opt.Excluding(x => x.Event);
                opt.Excluding(x => x.Account);
                opt.Excluding(x => x.Tenant);
                opt.ExcludingMissingMembers();
                return opt;
            });

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(3));

        }
    }

    [Fact]
    public async Task UpdateClientDetailsAsync_UpdateOnlyAccountManager_ReturnsAccountDetailsDtoAndUpdatesLastModifiedDate()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var users = TestData.UserFaker.Generate(5);
        var tenantUsers = users
            .Select(u => new TenantUser { UserId = u.Id, TenantId = tenant.Id })
            .ToList();
        var client = TestData.ClientsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .RuleFor(a => a.Account, _ => account)
            .RuleFor(a => a.ClientManagers, _ =>
                tenantUsers.Select(tu => new ClientManager
                {
                    TenantId = tu.TenantId,
                    UserId = tu.UserId
                }).Take(3).ToList())
            .Generate();

        var clientLastModifiedDate = client.LastModifiedDateTimeUtc;
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TenantUsers.AddRangeAsync(tenantUsers);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Clients.AddAsync(client);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        _userServiceMock
            .Setup(x => x.UserExistsInTenantAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(true);
        var clientDetailsUpdatedDate = DateTime.UtcNow;
        var accountManagersUnassignedDate = clientDetailsUpdatedDate.AddMilliseconds(1);
        var accountManagersAssignedDate = clientDetailsUpdatedDate.AddMilliseconds(2);
        _dateTimeServiceMock
            .SetupSequence(service => service.UtcNow)
            .Returns(clientDetailsUpdatedDate)
            .Returns(accountManagersUnassignedDate)
            .Returns(accountManagersAssignedDate);

        var expectedActivityLogs = new List<AccountActivityLog>()
            {
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.ClientAccountManagerAssigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.Client,
                    Date = accountManagersAssignedDate
                },
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.ClientAccountManagerUnassigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.Client,
                    Date = accountManagersUnassignedDate
                }
            };

        // new account managers ids contain 2 new ids and one existing id
        var newClientManagersIds = tenantUsers.Select(tu => tu.UserId).TakeLast(3).ToList();

        var request = (client.Adapt<UpdateClientDto>()) with
        {
            ClientManagersIds = newClientManagersIds
        };

        // Act
        var actualResult =
            await _accountService.UpdateClientDetailsAsync(client.AccountId, request);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var accountDetailsDto, out _);
            accountDetailsDto.Should().NotBeNull();
            accountDetailsDto.Should().BeEquivalentTo(request, opt => opt.ExcludingMissingMembers());
            var accountManagersIds = accountDetailsDto.Client!.ClientManagers.Select(sp => sp.UserId);
            accountManagersIds.Should().BeEquivalentTo(newClientManagersIds);
            var actualActivityLogs = await _dbContextMock.AccountActivityLogs.OrderByDescending(a => a.Date).ToListAsync();
            actualActivityLogs.Should().NotBeNull();
            actualActivityLogs.Should().BeEquivalentTo(expectedActivityLogs, opt =>
            {
                opt.WithStrictOrdering();
                opt.Excluding(x => x.Event);
                opt.Excluding(x => x.Account);
                opt.Excluding(x => x.Tenant);
                opt.ExcludingMissingMembers();
                return opt;
            });

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(3));

            accountDetailsDto.Client.LastModifiedDateTimeUtc.Should().BeAfter(clientLastModifiedDate!.Value);
        }
    }

    [Fact]
    public async Task UpdateClientDetailsAsync_UpdateClientDetailsWithoutAccountManagers_ReturnsAccountDetailsDtoAndCapturesActivityLog()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var users = TestData.UserFaker.Generate(5);
        var tenantUsers = users
            .Select(u => new TenantUser { UserId = u.Id, TenantId = tenant.Id })
            .ToList();
        var client = TestData.ClientsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .RuleFor(a => a.Account, _ => account)
            .RuleFor(a => a.ClientManagers, _ =>
                tenantUsers.Select(tu => new ClientManager
                {
                    TenantId = tu.TenantId,
                    UserId = tu.UserId
                }).Take(3).ToList())
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TenantUsers.AddRangeAsync(tenantUsers);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Clients.AddAsync(client);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        _userServiceMock
            .Setup(x => x.UserExistsInTenantAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(true);
        var currentDate = DateTime.UtcNow;
        var accountManagersUnassignedDate = currentDate.AddMilliseconds(1);
        var accountManagersAssignedDate = currentDate.AddMilliseconds(2);
        _dateTimeServiceMock
            .SetupSequence(service => service.UtcNow)
            .Returns(currentDate)
            .Returns(accountManagersUnassignedDate)
            .Returns(accountManagersAssignedDate);

        var expectedActivityLogs = new List<AccountActivityLog>()
            {
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.ClientAccountManagerAssigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.Client,
                    Date = accountManagersAssignedDate
                },
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.ClientAccountManagerUnassigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.Client,
                    Date = accountManagersUnassignedDate
                }
            };

        // new account managers ids contain 2 new ids and one existing id
        var newClientManagersIds = tenantUsers.Select(tu => tu.UserId).TakeLast(3).ToList();

        var adaptedClient = (client.Adapt<UpdateClientDto>()) with { ClientManagersIds = newClientManagersIds };
        // Act
        var actualResult =
            await _accountService.UpdateClientDetailsAsync(client.AccountId, adaptedClient);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var accountDetailsDto, out _);
            accountDetailsDto.Should().NotBeNull();
            accountDetailsDto.Should().BeEquivalentTo(adaptedClient, opt => opt.ExcludingMissingMembers());
            var accountManagersIds = accountDetailsDto.Client!.ClientManagers.Select(sp => sp.UserId);
            accountManagersIds.Should().BeEquivalentTo(newClientManagersIds);

            var actualActivityLogs = await _dbContextMock.AccountActivityLogs.OrderByDescending(a => a.Date).ToListAsync();
            actualActivityLogs.Should().NotBeNull();
            actualActivityLogs.Should().BeEquivalentTo(expectedActivityLogs, opt =>
            {
                opt.WithStrictOrdering();
                opt.Excluding(x => x.Event);
                opt.Excluding(x => x.Account);
                opt.Excluding(x => x.Tenant);
                opt.ExcludingMissingMembers();
                return opt;
            });

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(3));
        }
    }
}

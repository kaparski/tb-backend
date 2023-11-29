using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Fact]
    public async Task UpdateAccountProfileAsync_NoPhonesNoSalepersonsValidUpdateRequest_ReturnsAccountDetailsDtoAndCapturesActivityLog()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
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

        var request = TestData.UpdateAccountProfileDtoFaker.Generate();

        // Act
        var actualResult =
            await _accountService.UpdateAccountProfileAsync(account.Id, request);

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
            actualActivityLog?.EventType.Should().Be(AccountEventType.AccountProfileUpdated);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);
            actualActivityLog?.AccountId.Should().Be(account.Id);
            actualActivityLog?.AccountPartType.Should().Be(AccountPartActivityType.General);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_UpdatePhones_ReturnsAccountDetailsDtoAndCapturesActivityLog()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .RuleFor(a => a.Phones, _ => TestData.PhoneFaker.Generate(3))
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
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

        var phoneToAdd = TestData.CreateUpdatePhoneDtoFaker.Generate();
        var phoneToUpdate = TestData.CreateUpdatePhoneDtoFaker
            .RuleFor(p => p.Id, _ => account.Phones.Last().Id)
            .Generate();
        var newPhones = new List<CreateUpdatePhoneDto> { phoneToUpdate, phoneToAdd };

        var request = TestData.UpdateAccountProfileDtoFaker
            .RuleFor(x => x.Phones, _ => newPhones)
            .Generate();

        // Act
        var actualResult =
            await _accountService.UpdateAccountProfileAsync(account.Id, request);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var accountDetailsDto, out _);
            accountDetailsDto.Should().NotBeNull();
            accountDetailsDto.Should().BeEquivalentTo(request,
                opt => opt.ExcludingMissingMembers());
            accountDetailsDto?.Phones.Should().BeEquivalentTo(newPhones,
                opt => opt.ExcludingMissingMembers());

            var actualActivityLog = await _dbContextMock.AccountActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(AccountEventType.AccountProfileUpdated);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);
            actualActivityLog?.AccountId.Should().Be(account.Id);
            actualActivityLog?.AccountPartType.Should().Be(AccountPartActivityType.General);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_UpdateSalepersons_ReturnsAccountDetailsDtoAndCapturesActivityLog()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var users = TestData.UserFaker.Generate(5);
        var tenantUsers = users
            .Select(u => new TenantUser { UserId = u.Id, TenantId = tenant.Id })
            .ToList();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .RuleFor(a => a.Salespersons, _ =>
                tenantUsers.Select(tu => new AccountSalesperson
                {
                    TenantId = tu.TenantId,
                    UserId = tu.UserId
                }).Take(3).ToList())
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TenantUsers.AddRangeAsync(tenantUsers);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        _userServiceMock
            .Setup(x => x.UserExistsInTenantAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(true);

        var accountProfileUpdatedDate = DateTime.UtcNow;
        var salespersonUnAssignedDate = accountProfileUpdatedDate.AddMilliseconds(1);
        var salespersonAssignedDate = accountProfileUpdatedDate.AddMilliseconds(2);
        _dateTimeServiceMock
            .SetupSequence(service => service.UtcNow)
            .Returns(accountProfileUpdatedDate)
            .Returns(salespersonUnAssignedDate)
            .Returns(salespersonAssignedDate);

        var expectedActivityLogs = new List<AccountActivityLog>()
            {
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.SalespersonAssigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.General,
                    Date = salespersonAssignedDate
                },
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.SalespersonUnassigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.General,
                    Date = salespersonUnAssignedDate
                },
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.AccountProfileUpdated,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.General,
                    Date = accountProfileUpdatedDate
                },
            };

        // new saleperson ids contain 2 new ids and one existing id
        var newSalepersonIds = tenantUsers.Select(tu => tu.UserId).TakeLast(3).ToList();

        var request = TestData.UpdateAccountProfileDtoFaker
            .RuleFor(x => x.SalespersonIds, _ => newSalepersonIds)
            .Generate();

        // Act
        var actualResult =
            await _accountService.UpdateAccountProfileAsync(account.Id, request);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var accountDetailsDto, out _);
            accountDetailsDto.Should().NotBeNull();
            accountDetailsDto.Should().BeEquivalentTo(request, opt => opt.ExcludingMissingMembers());
            var salepersonIds = accountDetailsDto.Salespersons.Select(sp => sp.Id);
            salepersonIds.Should().BeEquivalentTo(newSalepersonIds);

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
    public async Task UpdateAccountProfileAsync_UpdateOnlySalepersons_ReturnsAccountDetailsDtoAndCapturesActivityLog()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var users = TestData.UserFaker.Generate(5);
        var tenantUsers = users
            .Select(u => new TenantUser { UserId = u.Id, TenantId = tenant.Id })
            .ToList();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .RuleFor(a => a.Salespersons, _ =>
                tenantUsers.Select(tu => new AccountSalesperson
                {
                    TenantId = tu.TenantId,
                    UserId = tu.UserId
                }).Take(3).ToList())
            .Generate();

        var lastModifiedDate = account.LastModifiedDateTimeUtc;
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TenantUsers.AddRangeAsync(tenantUsers);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        _userServiceMock
            .Setup(x => x.UserExistsInTenantAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(true);
        var currentDate = DateTime.UtcNow;
        var salespersonUnAssignedDate = currentDate.AddMilliseconds(1);
        var salespersonAssignedDate = currentDate.AddMilliseconds(2);
        _dateTimeServiceMock
            .SetupSequence(service => service.UtcNow)
            .Returns(currentDate)
            .Returns(salespersonUnAssignedDate)
            .Returns(salespersonAssignedDate);

        // new saleperson ids contain 2 new ids and one existing id
        var newSalepersonIds = tenantUsers.Select(tu => tu.UserId).TakeLast(3).ToList();

        var expectedActivityLogs = new List<AccountActivityLog>()
            {
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.SalespersonAssigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.General,
                    Date = salespersonAssignedDate
                },
                new AccountActivityLog()
                {
                    AccountId = account.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.SalespersonUnassigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.General,
                    Date = salespersonUnAssignedDate
                }
            };

        var adaptedAccount = (account.Adapt<UpdateAccountProfileDto>()) with { SalespersonIds = newSalepersonIds };

        // Act
        var actualResult =
            await _accountService.UpdateAccountProfileAsync(account.Id, adaptedAccount);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var accountDetailsDto, out _);
            accountDetailsDto.Should().NotBeNull();
            accountDetailsDto.Should().BeEquivalentTo(adaptedAccount, opt => opt.ExcludingMissingMembers());
            var salepersonIds = accountDetailsDto.Salespersons.Select(sp => sp.Id);
            salepersonIds.Should().BeEquivalentTo(newSalepersonIds);

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

            accountDetailsDto.LastModifiedDateTimeUtc.Should().BeAfter(lastModifiedDate!.Value);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(3));
        }
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_AccountDoesNotExist_ReturnsNotFound()
    {
        // Act
        var actualResult = await _accountService.UpdateAccountProfileAsync(
            Guid.NewGuid(), new UpdateAccountProfileDto());

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
        actualResult.IsT2.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_AccountDoesNotExistInTheTenant_ReturnsNotFound()
    {
        // Arrange
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenants[^1].Id)
            .Generate();

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[0].Id);

        // Act
        var actualResult = await _accountService.UpdateAccountProfileAsync(
            account.Id, new UpdateAccountProfileDto());

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
        actualResult.IsT2.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_SalepersonsFromAnotherTenant_ReturnsInvalidOperation()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var users = TestData.UserFaker.Generate(5);
        var tenantUsers = users
            .Select(u => new TenantUser { UserId = u.Id, TenantId = tenants[0].Id })
            .ToList();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenants[1].Id)
            .Generate();

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TenantUsers.AddRangeAsync(tenantUsers);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenants[1].Id);

        _userServiceMock
            .Setup(x => x.UserExistsInTenantAsync(
                It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(false);

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // new saleperson ids contain 2 new ids and one existing id
        var newSalepersonIds = tenantUsers.Select(tu => tu.UserId);

        var request = TestData.UpdateAccountProfileDtoFaker
            .RuleFor(x => x.SalespersonIds, _ => newSalepersonIds)
            .Generate();

        // Act
        var actualResult =
            await _accountService.UpdateAccountProfileAsync(account.Id, request);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeFalse();
            actualResult.IsT2.Should().BeTrue();
            actualResult.TryPickT2(out var error, out _);
            error?.ParamName.Should().Be("salespersonIds");
        }
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_AddPhoneThatBelongsToAnotherAccount_ReturnsInvalidOperation()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        var phones = TestData.PhoneFaker
            .RuleFor(p => p.TenantId, _ => tenant.Id)
            .RuleFor(p => p.AccountId, _ => Guid.NewGuid())
            .Generate(3);

        await _dbContextMock.AccountPhones.AddRangeAsync(phones);
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
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

        var phonesToAdd = phones.Adapt<List<CreateUpdatePhoneDto>>();

        var request = TestData.UpdateAccountProfileDtoFaker
            .RuleFor(x => x.Phones, _ => phonesToAdd)
            .Generate();

        // Act
        var actualResult =
            await _accountService.UpdateAccountProfileAsync(account.Id, request);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeFalse();
            actualResult.IsT2.Should().BeTrue();
            actualResult.TryPickT2(out var error, out _);
            error?.ParamName.Should().Be(nameof(request.Phones).ToLower());
        }
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_UpdateAccountWithExistingName_ReturnsNameAlreadyExists()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var accounts = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddRangeAsync(accounts);
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

        var request = TestData.UpdateAccountProfileDtoFaker
            .RuleFor(x => x.Name, _ => accounts[1].Name)
            .Generate();

        // Act
        var actualResult =
            await _accountService.UpdateAccountProfileAsync(accounts[0].Id, request);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeFalse();
            actualResult.IsT2.Should().BeTrue();
            actualResult.TryPickT2(out var error, out _);
            error?.ParamName.Should().Be(nameof(request.Name).ToLower());
        }
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_UpdateAccountWithExistingWebsite_ReturnsInvalidOperation()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var accounts = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddRangeAsync(accounts);
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

        var request = TestData.UpdateAccountProfileDtoFaker
            .RuleFor(x => x.Website, _ => accounts[1].Website)
            .Generate();

        // Act
        var actualResult =
            await _accountService.UpdateAccountProfileAsync(accounts[0].Id, request);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeFalse();
            actualResult.IsT2.Should().BeTrue();
            actualResult.TryPickT2(out var error, out _);
            error?.ParamName.Should().Be(nameof(request.Website).ToLower());
        }
    }

    [Fact]
    public async Task UpdateAccountProfileAsync_UpdateAccountWithNonExistingNaics_ReturnsInvalidOperation()
    {
        // Arrange

        var tenant = TestData.TenantFaker.Generate();
        var accounts = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddRangeAsync(accounts);
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

        var request = TestData.UpdateAccountProfileDtoFaker
            .RuleFor(x => x.PrimaryNaicsCode, _ => 999999)
            .Generate();

        // Act
        var actualResult =
            await _accountService.UpdateAccountProfileAsync(accounts[0].Id, request);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeFalse();
            actualResult.IsT2.Should().BeTrue();
            actualResult.TryPickT2(out var error, out _);
            error?.ParamName.Should().Be("primaryNaics");
        }
    }
}

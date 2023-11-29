using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Common.Models;
using TaxBeacon.Common.Enums.Accounts.Activities;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Fact]
    public async Task CreateAccountAsync_SalepersonsFromAnotherTenant_ReturnsInvalidOperation()
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

        var request = TestData.CreateAccountDtoFaker
            .RuleFor(x => x.SalespersonIds, _ => newSalepersonIds)
            .Generate();

        // Act
        var actualResult =
            await _accountService.CreateAccountAsync(request, AccountInfoType.Full);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
            actualResult.TryPickT1(out var error, out _);
            error?.ParamName.Should().Be("salespersonIds");
        }
    }

    [Fact]
    public async Task CreateAccountAsync_AddPhoneThatBelongsToAnotherAccount_ReturnsInvalidOperation()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        var phones = TestData.PhoneFaker
            .RuleFor(p => p.AccountId, _ => Guid.NewGuid())
            .RuleFor(p => p.TenantId, _ => tenant.Id)
            .Generate(3);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountPhones.AddRangeAsync(phones);
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

        var request = TestData.CreateAccountDtoFaker
            .RuleFor(x => x.Phones, _ => phonesToAdd)
            .Generate();

        // Act
        var actualResult =
            await _accountService.CreateAccountAsync(request, AccountInfoType.Full);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
            actualResult.TryPickT1(out var error, out _);
            error?.ParamName.Should().Be(nameof(request.Phones).ToLower());
        }
    }

    [Fact]
    public async Task CreateAccountAsync_AccountWithAccountIdAlreadyExists_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var existingAccount = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(existingAccount);
        await _dbContextMock.SaveChangesAsync();

        var request = TestData.CreateAccountDtoFaker
            .RuleFor(r => r.AccountId, _ => existingAccount.AccountId)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        //Act
        var actualResult =
            await _accountService.CreateAccountAsync(request, AccountInfoType.Full);

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.TryPickT1(out var error, out _);
            error?.ParamName.Should().BeEquivalentTo(nameof(request.AccountId));
            error?.Message.Should().Be("account with the same account ID already exists");
        }
    }

    [Fact]
    public async Task CreateAccountAsync_CreateAccountWithExistingName_ReturnsNameAlreadyExists()
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

        var request = TestData.CreateAccountDtoFaker
            .RuleFor(x => x.Name, _ => accounts[1].Name)
            .Generate();

        // Act
        var actualResult =
            await _accountService.CreateAccountAsync(request, AccountInfoType.Full);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
            actualResult.TryPickT1(out var error, out _);
            error?.ParamName.Should().Be(nameof(request.Name).ToLower());
        }
    }

    [Fact]
    public async Task CreateAccountAsync_CreateAccountWithExistingWebsite_ReturnsInvalidOperation()
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

        var request = TestData.CreateAccountDtoFaker
            .RuleFor(x => x.Website, _ => accounts[1].Website)
            .Generate();

        // Act
        var actualResult =
            await _accountService.CreateAccountAsync(request, AccountInfoType.Full);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
            actualResult.TryPickT1(out var error, out _);
            error?.ParamName.Should().Be(nameof(request.Website).ToLower());
        }
    }

    [Fact]
    public async Task CreateAccountAsync_CreateAccountWithNonExistingNaics_ReturnsInvalidOperation()
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

        _naicsServiceMock
            .Setup(x => x.IsNaicsValidAsync(It.IsAny<int>(), default))
            .ReturnsAsync(false);

        var request = TestData.CreateAccountDtoFaker
            .RuleFor(x => x.PrimaryNaicsCode, _ => 999999)
            .Generate();

        // Act
        var actualResult =
            await _accountService.CreateAccountAsync(request, AccountInfoType.Full);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
            actualResult.TryPickT1(out var error, out _);
            error?.ParamName.Should().Be("primaryNaics");
        }
    }

    [Fact]
    public async Task CreateAccountAsync_CreateAccountDto_ReturnsAccountDetailsAndCapturesActivityLog()
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

        var profileCreatedDate = DateTime.UtcNow;
        var managerAssignedDate = profileCreatedDate.AddMilliseconds(1);
        var salespersonAssignedDate = profileCreatedDate.AddMilliseconds(2);
        _dateTimeServiceMock
            .SetupSequence(service => service.UtcNow)
            .Returns(profileCreatedDate)
            .Returns(salespersonAssignedDate)
            .Returns(managerAssignedDate);

        var newSalespersonIds = tenantUsers.Select(tu => tu.UserId).TakeLast(3).ToList();
        var request = TestData.CreateAccountDtoFaker
            .RuleFor(x => x.Client, _ => TestData.CreateClientDtoFaker.Generate())
            .RuleFor(x => x.Referral,  _ => null)
            .RuleFor(x => x.SalespersonIds, _ => newSalespersonIds)
            .Generate();

        // Act
        var actualResult = await _accountService.CreateAccountAsync(request, AccountInfoType.Client);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var accountDetailsDto, out _);
            accountDetailsDto.Should().NotBeNull();
            accountDetailsDto.Should().BeEquivalentTo(request,
            opt => opt.ExcludingMissingMembers());

            var expectedActivityLogs = new List<AccountActivityLog>()
            {
                new AccountActivityLog()
                {
                    AccountId = accountDetailsDto.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.ClientAccountManagerAssigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.Client,
                    Date = managerAssignedDate
                },
                new AccountActivityLog()
                {
                    AccountId = accountDetailsDto.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.ClientAccountCreated,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.Client,
                    Date = profileCreatedDate
                },
                new AccountActivityLog()
                {
                    AccountId = accountDetailsDto.Id,
                    TenantId = tenant.Id,
                    EventType = AccountEventType.SalespersonAssigned,
                    Revision = 1,
                    AccountPartType = AccountPartActivityType.General,
                    Date = salespersonAssignedDate
                },
            };

            var actualActivityLog = await _dbContextMock.AccountActivityLogs.OrderByDescending(a => a.Date).ToListAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog.Should().BeEquivalentTo(expectedActivityLogs, opt =>
            {
                opt.WithStrictOrdering();
                opt.Excluding(x => x.Event);
                opt.Excluding(x => x.Account);
                opt.Excluding(x => x.Tenant);
                opt.Excluding(x => x.Date);
                opt.ExcludingMissingMembers();
                return opt;
            });

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(6));
        }
    }
}

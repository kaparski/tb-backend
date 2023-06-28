﻿using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.Accounts.UnitTests.Accounts;

public sealed class AccountsServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<AccountService>> _accountServiceLoggerMock;
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly IAccountService _accountService;

    public AccountsServiceTests()
    {
        _currentUserServiceMock = new();
        _entitySaveChangesInterceptorMock = new();
        _accountServiceLoggerMock = new();
        _csvMock = new();
        _xlsxMock = new();
        _listToFileConverters = new();
        _dateTimeServiceMock = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        _listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(AccountsServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _accountService = new AccountService(
            _accountServiceLoggerMock.Object,
            _dbContextMock,
            _currentUserServiceMock.Object,
            _dateTimeServiceMock.Object,
            _listToFileConverters.Object);
    }

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
    public void AccountsView_ListOfColumnsMatchesAccountViewEntity()
    {
        // Arrange
        var usersViewScript = File.ReadAllText("../../../../../migration-scripts/AccountsView.sql");

        var fieldsAsString = new Regex(@"select((.|\n)*)from", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Match(usersViewScript)
            .Groups[1]
            .Value;

        var fields = new Regex(@"(\w+),?[\r\n]", RegexOptions.IgnoreCase | RegexOptions.Multiline)
            .Matches(fieldsAsString)
            .Select(m => m.Groups[1].Value)
            .ToArray();

        var props = typeof(AccountView).GetProperties()
            .Select(p => p.Name)
            .ToArray();

        // Assert
        using (new AssertionScope())
        {
            fields.Should().BeEquivalentTo(props);
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportAccountsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var accounts = TestData.AccountsViewFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.AccountsView.AddRangeAsync(accounts);
        await _dbContextMock.SaveChangesAsync();
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        // Act
        _ = await _accountService.ExportAccountsAsync(fileType);

        // Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x => x.Convert(It.IsAny<List<AccountExportDto>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x => x.Convert(It.IsAny<List<AccountExportDto>>()), Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task GetAccountDetailsByIdAsync_AccountExists_ReturnsAccountDetailsDto()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var accounts = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate(3);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddRangeAsync(accounts);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _accountService.GetAccountDetailsByIdAsync(accounts[0].Id);

        // Assert
        actualResult.TryPickT0(out var actualAccount, out _).Should().BeTrue();
        actualAccount.Should().BeEquivalentTo(accounts[0], opt => opt.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetAccountDetailsByIdAsync_NonExistingTenantId_ReturnsNotFound()
    {
        // Act
        var actualResult = await _accountService.GetAccountDetailsByIdAsync(Guid.NewGuid());

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
        var actualResult = await _accountService.GetAccountDetailsByIdAsync(accounts[0].Id);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Fact]
    public async Task GetClientDetailsByIdAsync_ClientExists_ReturnsClienDetailsDto()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var client = TestData.ClientsFaker
            .RuleFor(c => c.AccountId, _ => account.Id)
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Clients.AddAsync(client);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _accountService.GetClientDetailsByIdAsync(client.AccountId);

        // Assert
        actualResult.TryPickT0(out var actualClient, out _).Should().BeTrue();
        actualClient.Should().BeEquivalentTo(client, opt => opt.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetClientDetailsByIdAsync_NonExistingTenantId_ReturnsNotFound()
    {
        // Act
        var actualResult = await _accountService.GetClientDetailsByIdAsync(Guid.NewGuid());

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Fact]
    public async Task GetClientDetailsByIdAsync_ClientTenantIdNotEqualCurrentUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var account = TestData.AccountsViewFaker
            .RuleFor(a => a.TenantId, _ => tenants[0].Id)
            .Generate();
        var client = TestData.ClientsFaker
            .RuleFor(a => a.AccountId, _ => account.Id)
            .RuleFor(a => a.TenantId, _ => tenants[^1].Id)
            .Generate();

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.AccountsView.AddAsync(account);
        await _dbContextMock.Clients.AddAsync(client);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[0].Id);

        // Act
        var actualResult = await _accountService.GetClientDetailsByIdAsync(client.AccountId);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Fact]
    public async Task GetReferralDetailsByIdAsync_ReferralExists_ReturnsClienDetailsDto()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountsFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var referral = TestData.ReferralsFaker
            .RuleFor(c => c.AccountId, _ => account.Id)
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Referrals.AddAsync(referral);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _accountService.GetReferralDetailsByIdAsync(referral.AccountId);

        // Assert
        actualResult.TryPickT0(out var actualReferral, out _).Should().BeTrue();
        actualReferral.Should().BeEquivalentTo(referral, opt => opt.ExcludingMissingMembers());
    }

    [Fact]
    public async Task GetReferralDetailsByIdAsync_NonExistingTenantId_ReturnsNotFound()
    {
        // Act
        var actualResult = await _accountService.GetReferralDetailsByIdAsync(Guid.NewGuid());

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Fact]
    public async Task GetReferraltDetailsByIdAsync_ReferralTenantIdNotEqualCurrentUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var account = TestData.AccountsViewFaker
            .RuleFor(a => a.TenantId, _ => tenants[0].Id)
            .Generate();
        var referral = TestData.ReferralsFaker
            .RuleFor(a => a.AccountId, _ => account.Id)
            .RuleFor(a => a.TenantId, _ => tenants[^1].Id)
            .Generate();

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.AccountsView.AddAsync(account);
        await _dbContextMock.Referrals.AddAsync(referral);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[0].Id);

        // Act
        var actualResult = await _accountService.GetReferralDetailsByIdAsync(referral.AccountId);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Faker<AccountView> AccountsViewFaker =
            new Faker<AccountView>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(a => a.State, f => f.PickRandom<State>())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.AccountType, f => f.Name.JobTitle());

        public static readonly Faker<Account> AccountsFaker =
            new Faker<Account>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Country, f => f.Address.Country())
                .RuleFor(a => a.State, f => f.PickRandom<State>())
                .RuleFor(a => a.City, f => f.Address.City())
                .RuleFor(a => a.StreetAddress1, f => f.Address.StreetAddress())
                .RuleFor(a => a.Zip, f => f.Random.Number(10000, 9999999).ToString())
                .RuleFor(a => a.County, f => f.Address.County())
                .RuleFor(a => a.Phone, f => f.Random.Number(100000000, 99999999).ToString())
                .RuleFor(a => a.Fax, f => f.Random.Number(100000000, 99999999).ToString());

        public static readonly Faker<Client> ClientsFaker =
           new Faker<Client>()
               .RuleFor(a => a.CreatedDateTimeUtc, _ => DateTime.UtcNow)
               .RuleFor(a => a.ReactivationDateTimeUtc, _ => DateTime.UtcNow)
               .RuleFor(a => a.DeactivationDateTimeUtc, _ => DateTime.UtcNow)
               .RuleFor(a => a.AnnualRevenue, f => f.Finance.Amount())
               .RuleFor(a => a.FoundationYear, f => f.Random.Number(1900, 2023))
               .RuleFor(a => a.State, f => f.PickRandom("Client", "Client prospect"))
               .RuleFor(a => a.EmployeeCount, f => f.Random.Number(0, 1000))
               .RuleFor(a => a.Status, f => f.PickRandom<Status>());

        public static readonly Faker<Referral> ReferralsFaker =
           new Faker<Referral>()
               .RuleFor(a => a.CreatedDateTimeUtc, _ => DateTime.UtcNow)
               .RuleFor(a => a.State, f => f.PickRandom("Referral prospect", "Referral partner"))
               .RuleFor(a => a.Status, f => f.PickRandom<Status>());

        public static readonly Faker<Tenant> TenantFaker =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);
    }
}

using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;
using TaxBeacon.Accounts.Services.Contacts;
using TaxBeacon.Accounts.Services.Contacts.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Entities.Accounts;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.Accounts.UnitTests.Contacts;

public class ContactServiceTests
{
    private readonly TaxBeaconDbContext _dbContext;
    private readonly IContactService _contactService;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public ContactServiceTests()
    {
        _currentUserServiceMock = new();
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        _dbContext = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(ContactServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _contactService = new ContactService(_currentUserServiceMock.Object, _dbContext);
    }

    [Fact]
    public async Task QueryContacts_AccountExists_ReturnsContacts()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        TestData.TestContact.RuleFor(x => x.Tenant, tenant);
        TestData.TestAccount.RuleFor(x => x.Tenant, tenant);
        var account = TestData.TestAccount.Generate();

        TestData.TestContact.RuleFor(x => x.Account, account);
        await _dbContext.Tenants.AddAsync(tenant);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        var items = TestData.TestContact.Generate(3);
        await _dbContext.Contacts.AddRangeAsync(items);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var oneOf = await _contactService.QueryContactsAsync(items[0].Account.Id);

        // Assert
        using (new AssertionScope())
        {
            oneOf.IsT0.Should().BeTrue();
            var result = oneOf.AsT0.Value;
            result.Should().HaveCount(3);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());
            }
        }
    }

    [Fact]
    public async Task QueryContacts_AccountDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        TestData.TestContact.RuleFor(x => x.Tenant, tenant);
        TestData.TestAccount.RuleFor(x => x.Tenant, tenant);
        var account = TestData.TestAccount.Generate();

        TestData.TestContact.RuleFor(x => x.Account, account);
        await _dbContext.Tenants.AddAsync(tenant);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        var items = TestData.TestContact.Generate(3);
        await _dbContext.Contacts.AddRangeAsync(items);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var oneOf = await _contactService.QueryContactsAsync(new Guid());

        // Assert
        using (new AssertionScope())
        {
            oneOf.IsT1.Should().BeTrue();
            var result = oneOf.AsT1;
            result.Should().BeOfType<NotFound>();
        }
    }

    [Fact]
    public async Task GetContactDetailsAsync_ContactExists_ReturnsContact()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        TestData.TestContact.RuleFor(x => x.Tenant, tenant);
        TestData.TestAccount.RuleFor(x => x.Tenant, tenant);
        var account = TestData.TestAccount.Generate();

        TestData.TestContact.RuleFor(x => x.Account, account);
        await _dbContext.Tenants.AddAsync(tenant);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        var item = TestData.TestContact.Generate();
        await _dbContext.Contacts.AddRangeAsync(item);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var oneOf = await _contactService.GetContactDetailsAsync(item.Account.Id, item.Id);

        // Assert
        using (new AssertionScope())
        {
            oneOf.IsT0.Should().BeTrue();
            var dto = oneOf.AsT0;
            dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());
        }
    }

    [Fact]
    public async Task GetContactDetailsAsync_ContactDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();
        TestData.TestContact.RuleFor(x => x.Tenant, tenant);
        TestData.TestAccount.RuleFor(x => x.Tenant, tenant);
        var account = TestData.TestAccount.Generate();

        TestData.TestContact.RuleFor(x => x.Account, account);
        await _dbContext.Tenants.AddAsync(tenant);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(tenant.Id);

        var items = TestData.TestContact.Generate(3);
        await _dbContext.Contacts.AddRangeAsync(items);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var oneOf = await _contactService.QueryContactsAsync(new Guid());

        // Assert
        using (new AssertionScope())
        {
            oneOf.IsT1.Should().BeTrue();
            var result = oneOf.AsT1;
            result.Should().BeOfType<NotFound>();
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();

        public static readonly Faker<Contact> TestContact =
            new Faker<Contact>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.FirstName, f => f.Person.FirstName)
                .RuleFor(t => t.LastName, f => f.Person.LastName)
                .RuleFor(t => t.Email, t => t.Person.Email)
                .RuleFor(t => t.Type, t => t.PickRandom("Client", "Referral Partner", "Client Partner"))
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(t => t.Status, t => Status.Active)
                .RuleFor(t => t.Phone, t => t.Person.Phone);

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => TestTenantId)
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<Account> TestAccount =
            new Faker<Account>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Website, f => f.Internet.Url())
                .RuleFor(t => t.Name, f => f.Person.FullName);
    }
}

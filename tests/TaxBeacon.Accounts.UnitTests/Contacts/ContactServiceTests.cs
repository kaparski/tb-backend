using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf.Types;
using System.Diagnostics.CodeAnalysis;
using TaxBeacon.Accounts.Contacts;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.Accounts.UnitTests.Contacts;

public class ContactServiceTests
{
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<ILogger<ContactService>> _loggerMock;
    private readonly TaxBeaconDbContext _dbContext;
    private readonly IContactService _contactService;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters;

    public ContactServiceTests()
    {
        _loggerMock = new();
        _dateTimeServiceMock = new();
        _currentUserServiceMock = new();

        _csvMock = new();
        _xlsxMock = new();
        _listToFileConverters = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        _listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        _dbContext = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(ContactServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _contactService = new ContactService(_loggerMock.Object, _currentUserServiceMock.Object, _dbContext, _dateTimeServiceMock.Object, _listToFileConverters.Object);
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
        var result = _contactService.QueryContacts();

        // Assert
        using (new AssertionScope())
        {
            result.Should().HaveCount(3);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());
            }
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
        var oneOf = await _contactService.GetContactDetailsAsync(item.Id, item.Account.Id, default);

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

        var items = TestData.TestContact.Generate();
        await _dbContext.Contacts.AddRangeAsync(items);
        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.SaveChangesAsync();

        // Act
        var oneOf = await _contactService.GetContactDetailsAsync(new Guid(), new Guid(), default);

        // Assert
        using (new AssertionScope())
        {
            oneOf.IsT1.Should().BeTrue();
            var result = oneOf.AsT1;
            result.Should().BeOfType<NotFound>();
        }
    }

    [Fact]
    public async Task UpdateContactStatusAsync_ActiveStatusAndContactId_Succeeds()
    {
        //Arrange
        var account = TestData.TestAccount.Generate();
        var contact = TestData.TestContact.Generate();
        var currentDate = DateTime.UtcNow;

        contact.AccountId = account.Id;
        contact.TenantId = TestData.TestTenantId;

        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.Contacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        //Act
        var actualResult = await _contactService.UpdateContactStatusAsync(contact.Id, contact.AccountId, Status.Active);

        //Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var dto, out _).Should().BeTrue();
            dto.Status.Should().Be(Status.Active);
            dto.DeactivationDateTimeUtc.Should().BeNull();
            dto.ReactivationDateTimeUtc.Should().Be(currentDate);
        }
    }

    [Fact]
    public async Task UpdateContactStatusAsync_DeactivatedStatusAndContactId_Succeeds()
    {
        //Arrange
        var account = TestData.TestAccount.Generate();
        var contact = TestData.TestContact.Generate();
        var currentDate = DateTime.UtcNow;

        contact.AccountId = account.Id;
        contact.TenantId = TestData.TestTenantId;

        await _dbContext.Accounts.AddRangeAsync(account);
        await _dbContext.Contacts.AddAsync(contact);
        await _dbContext.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        //Act
        var actualResult = await _contactService.UpdateContactStatusAsync(contact.Id, contact.AccountId, Status.Deactivated);

        //Assert
        using (new AssertionScope())
        {
            (await _dbContext.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var dto, out _).Should().BeTrue();
            dto.Status.Should().Be(Status.Deactivated);
            dto.ReactivationDateTimeUtc.Should().BeNull();
            dto.DeactivationDateTimeUtc.Should().Be(currentDate);
        }
    }

    [Fact]
    public async Task UpdateContactStatusAsync_ContactIdNotInDb_ReturnNotFound()
    {
        //Act
        var actualResult = await _contactService.UpdateContactStatusAsync(Guid.NewGuid(), Guid.NewGuid(), Status.Deactivated);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out _, out _).Should().BeTrue();
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportContactsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        // Arrange
        var items = TestData.TestContact
            .RuleFor(a => a.TenantId, _ => TestData.TestTenantId)
            .Generate(5);

        await _dbContext.Contacts.AddRangeAsync(items);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(TestData.TestTenantId);

        // Act
        _ = await _contactService.ExportContactsAsync(fileType);

        // Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x => x.Convert(It.IsAny<List<ContactExportModel>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x => x.Convert(It.IsAny<List<ContactExportModel>>()), Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();

        public static readonly Faker<Contact> TestContact =
            new Faker<Contact>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.AccountId, f => Guid.NewGuid())
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
                .RuleFor(a => a.Id, f => Guid.NewGuid())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Name, f => f.Person.FullName)
                .RuleFor(a => a.Country, f => f.Address.Country());
    }
}

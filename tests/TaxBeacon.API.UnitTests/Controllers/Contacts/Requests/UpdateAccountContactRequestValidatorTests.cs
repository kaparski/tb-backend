using Bogus;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.API.Controllers.Contacts.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.Contacts.Requests;

public class UpdateAccountContactRequestValidatorTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly UpdateAccountContactRequestValidator _updateContactRequestValidator;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public UpdateAccountContactRequestValidatorTests()
    {
        _currentUserServiceMock = new();
        _httpContextAccessorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UpdateAccountContactRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _updateContactRequestValidator =
            new UpdateAccountContactRequestValidator(_httpContextAccessorMock.Object, _dbContextMock, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_ValidRequest_ShouldHaveNoErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate();
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Contact, _ => contact)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contact.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateContactRequest = TestData.UpdateAccountContactRequestFaker
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateContactRequest);

        // Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validation_EmptyFirstName_ShouldHaveErrors(string firstName)
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate();
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Contact, _ => contact)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contact.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateAccountContactRequest = TestData.UpdateAccountContactRequestFaker
            .RuleFor(c => c.FirstName, _ => firstName)
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateAccountContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.SecondaryEmail);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitle);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Phones);
    }

    [Fact]
    public async Task Validation_LongFirstName_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate();
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Contact, _ => contact)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contact.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateAccountContactRequest = TestData.UpdateAccountContactRequestFaker
            .RuleFor(r => r.FirstName, f => f.Random.String2(200))
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateAccountContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.SecondaryEmail);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitle);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Phones);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validation_EmptyLastName_ShouldHaveErrors(string lastName)
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate();
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Contact, _ => contact)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contact.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateAccountContactRequest = TestData.UpdateAccountContactRequestFaker
            .RuleFor(r => r.LastName, _ => lastName)
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateAccountContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.SecondaryEmail);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitle);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Phones);
    }

    [Fact]
    public async Task Validation_LongLastName_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate();
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Contact, _ => contact)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contact.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateAccountContactRequest = TestData.UpdateAccountContactRequestFaker
            .RuleFor(r => r.LastName, f => f.Random.String2(200))
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateAccountContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.LastName)
            .WithErrorMessage("The last name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.SecondaryEmail);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitle);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Phones);
    }

    [Fact]
    public async Task Validation_LongJobTitle_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate();
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Contact, _ => contact)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contact.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateAccountContactRequest = TestData.UpdateAccountContactRequestFaker
            .RuleFor(r => r.JobTitle, f => f.Random.String2(200))
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateAccountContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.JobTitle)
            .WithErrorMessage("The job title must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.SecondaryEmail);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Phones);
    }

    [Fact]
    public async Task Validation_LongSecondaryEmail_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate();
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Contact, _ => contact)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contact.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateAccountContactRequest = TestData.UpdateAccountContactRequestFaker
            .RuleFor(r => r.SecondaryEmail, f => f.Random.String2(200))
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateAccountContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.SecondaryEmail)
            .WithErrorMessage("The secondary email must contain no more than 64 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitle);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Phones);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validation_EmptyEmail_ShouldHaveErrors(string email)
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate();
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Contact, _ => contact)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contact.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateAccountContactRequest = TestData.UpdateAccountContactRequestFaker
            .RuleFor(r => r.Email, _ => email)
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateAccountContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.SecondaryEmail);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitle);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Phones);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("Aa.")]
    public async Task Validation_InvalidEmail_ShouldHaveErrors(string email)
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate();
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Contact, _ => contact)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contact.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateAccountContactRequest = TestData.UpdateAccountContactRequestFaker
            .RuleFor(r => r.Email, _ => email)
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateAccountContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.SecondaryEmail);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitle);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Phones);
    }

    [Fact]
    public async Task Validation_LongEmail_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate();
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Contact, _ => contact)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contact.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateAccountContactRequest = TestData.UpdateAccountContactRequestFaker
            .RuleFor(r => r.Email, f => f.Random.String2(65))
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateAccountContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Email)
            .WithErrorMessage("The email must contain no more than 64 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.SecondaryEmail);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitle);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Phones);
    }

    [Fact]
    public async Task Validation_ContactWithSuchEmailAlreadyExists_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contacts = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate(2);
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .Generate(2);
        accountContact[0].Contact = contacts[0];
        accountContact[1].Contact = contacts[1];

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddRangeAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contacts.Last().Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateAccountContactRequest = TestData.UpdateAccountContactRequestFaker
            .RuleFor(r => r.Email, _ => contacts.First().Email)
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateAccountContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Email)
            .WithErrorMessage("Contact with the same email already exists");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.SecondaryEmail);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitle);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Phones);
    }

    [Fact]
    public async Task Validation_InvalidContactType_ShouldHaveErrors()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .Generate();
        var accountContact = TestData.AccountContactFaker
            .RuleFor(l => l.TenantId, _ => tenant.Id)
            .RuleFor(l => l.AccountId, _ => account.Id)
            .RuleFor(l => l.Contact, _ => contact)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.AccountContacts.AddAsync(accountContact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("contactId", contact.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        var updateAccountContactRequest = TestData.UpdateAccountContactRequestFaker
            .RuleFor(r => r.Type, _ => ContactType.None)
            .Generate();

        // Act
        var actualResult = await _updateContactRequestValidator.TestValidateAsync(updateAccountContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Type);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.SecondaryEmail);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitle);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Phones);
    }

    private static class TestData
    {
        public static Faker<UpdateAccountContactRequest> UpdateAccountContactRequestFaker => new Faker<UpdateAccountContactRequest>()
            .RuleFor(c => c.FirstName, f => f.Person.FirstName)
            .RuleFor(c => c.LastName, f => f.Person.LastName)
            .RuleFor(c => c.Email, f => f.Person.Email)
            .RuleFor(c => c.SecondaryEmail, f => f.Person.Email)
            .RuleFor(c => c.JobTitle, f => f.Person.Company.Name)
            .RuleFor(c => c.Type, f =>
            {
                var list = ContactType.List.Except(new[] { ContactType.None });
                return f.PickRandom(list);
            });

        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static Faker<Account> AccountFaker =>
            new Faker<Account>()
                .RuleFor(a => a.Id, _ => Guid.NewGuid())
                .RuleFor(a => a.Website, f => f.Internet.Url())
                .RuleFor(a => a.Name, f => f.Company.CompanyName())
                .RuleFor(a => a.AccountId, f => f.Random.String(10))
                .RuleFor(a => a.Country, f => f.Address.Country());

        public static Faker<AccountContact> AccountContactFaker =>
            new Faker<AccountContact>()
                .RuleFor(a => a.Status, _ => Status.Active)
                .RuleFor(c => c.Type, f =>
                {
                    var list = ContactType.List.Except(new[] { ContactType.None }).Select(x => x.Name);
                    return f.PickRandom(list);
                });

        public static Faker<Contact> ContactFaker =>
            new Faker<Contact>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.FirstName, f => f.Person.FirstName)
                .RuleFor(t => t.LastName, f => f.Person.LastName)
                .RuleFor(t => t.Email, t => t.Person.Email)
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);
    }
}

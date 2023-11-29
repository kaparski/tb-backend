using Bogus;
using FluentValidation.TestHelper;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.API.Controllers.Contacts.Requests;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts.Entities;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.Contacts.Requests;

public sealed class CreateContactRequestValidatorTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly CreateContactRequestValidator _createContactRequestValidator;

    public CreateContactRequestValidatorTests()
    {
        _currentUserServiceMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(CreateContactRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _createContactRequestValidator =
            new CreateContactRequestValidator(_dbContextMock, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_ValidRequest_ShouldHaveNoErrors()
    {
        // Arrange
        var createContactRequest = TestData.CreateContactRequestFaker.Generate();

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

        // Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validation_EmptyFirstName_ShouldHaveErrors(string firstName)
    {
        // Arrange
        var createContactRequest = TestData.CreateContactRequestFaker
            .RuleFor(c => c.FirstName, _ => firstName)
            .Generate();

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

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
        var createContactRequest = TestData.CreateContactRequestFaker
            .RuleFor(r => r.FirstName, f => f.Random.String2(200))
            .Generate();

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.FirstName)
            .WithErrorMessage("The first name must contain no more than 100 characters");
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
        var createContactRequest = TestData.CreateContactRequestFaker
            .RuleFor(c => c.LastName, _ => lastName)
            .Generate();

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

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
        var createContactRequest = TestData.CreateContactRequestFaker
            .RuleFor(r => r.LastName, f => f.Random.String2(200))
            .Generate();

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

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
        var createContactRequest = TestData.CreateContactRequestFaker
            .RuleFor(r => r.JobTitle, f => f.Random.String2(200))
            .Generate();

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

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
        var createContactRequest = TestData.CreateContactRequestFaker
            .RuleFor(r => r.SecondaryEmail, f => f.Random.String2(200))
            .Generate();

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

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
        var createContactRequest = TestData.CreateContactRequestFaker
            .RuleFor(c => c.Email, _ => email)
            .Generate();

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

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
        var createContactRequest = TestData.CreateContactRequestFaker
            .RuleFor(c => c.Email, _ => email)
            .Generate();

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

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
        var createContactRequest = TestData.CreateContactRequestFaker
            .RuleFor(r => r.Email, f => f.Random.String2(200))
            .Generate();

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

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
        var createContactRequest = TestData.CreateContactRequestFaker
            .Generate();
        var contact = TestData.ContactFaker
            .RuleFor(c => c.TenantId, _ => tenant.Id)
            .RuleFor(c => c.Email, _ => createContactRequest.Email)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Contacts.AddAsync(contact);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

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
        var createContactRequest = TestData.CreateContactRequestFaker
            .RuleFor(r => r.Type, _ => ContactType.None)
            .Generate();

        // Act
        var actualResult = await _createContactRequestValidator.TestValidateAsync(createContactRequest);

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
        public static Faker<CreateContactRequest> CreateContactRequestFaker => new Faker<CreateContactRequest>()
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

        public static Faker<Contact> ContactFaker =>
            new Faker<Contact>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.FirstName, f => f.Person.FirstName)
                .RuleFor(t => t.LastName, f => f.Person.LastName)
                .RuleFor(t => t.Email, t => t.Person.Email)
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);
    }
}

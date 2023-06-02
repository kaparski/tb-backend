using Bogus;
using FluentValidation.TestHelper;
using Moq;
using TaxBeacon.API.Controllers.Users.Requests;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.UnitTests.Controllers.User.Requests;

public class CreateUserRequestValidatorTest
{
    private readonly CreateUserRequestValidator _createUserRequestValidator;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    public CreateUserRequestValidatorTest()
    {
        _currentUserServiceMock = new();
        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);
        _createUserRequestValidator = new CreateUserRequestValidator(_currentUserServiceMock.Object);
    }

    [Fact]
    public void Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    public void Validation_InvalidEmail_ShouldHaveErrors(string email)
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                email,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Fact]
    public void Validation_LongEmail_ShouldHaveMaxLengthError()
    {
        //Arrange
        var email = $"{new Faker().Random.String2(200)}@email.com";
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                email,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Email)
            .WithErrorMessage("The email must contain no more than 64 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidFirstName_ShouldHaveErrors(string firstName)
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                firstName,
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Fact]
    public void Validation_LongFirstName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var firstName = new Faker().Random.String2(115);
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                firstName,
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.FirstName)
            .WithErrorMessage("The first name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidLegalName_ShouldHaveErrors(string legalName)
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                legalName,
                f.Name.LastName(),
                f.Internet.Email(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Fact]
    public void Validation_LongLegalName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var legalName = new Faker().Random.String2(115);
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                legalName,
                f.Name.LastName(),
                f.Internet.Email(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult
            .ShouldHaveValidationErrorFor(r => r.LegalName)
            .WithErrorMessage("Legal name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidLastName_ShouldHaveErrors(string lastName)
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                lastName,
                f.Internet.Email(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
    }

    [Fact]
    public void Validation_LongLastName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var lastName = new Faker().Random.String2(115);
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                lastName,
                f.Internet.Email(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.LastName)
            .WithErrorMessage("The last name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
    }

    [Fact]
    public void Validation_DepartmentWithNoDivision_ShouldHaveDepartmentIdError()
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                null,
                Guid.NewGuid(),
                null,
                null,
                null))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.DepartmentId)
            .WithErrorMessage("Cannot set a department without a division.");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
    }

    [Fact]
    public void Validation_ServiceAreaWithNoDepartment_ShouldHaveServiceAreaIdError()
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                null,
                null,
                null,
                Guid.NewGuid(),
                null))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.JobTitleId)
            .WithErrorMessage("Cannot set a job title without a department.");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
    }

    [Fact]
    public void Validation_JobTitleWithNoDepartment_ShouldHaveJobTitleIdError()
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                null,
                null,
                Guid.NewGuid(),
                null,
                null))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.ServiceAreaId)
            .WithErrorMessage("Cannot set a service area without a department.");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
    }

    [Fact]
    public void Validation_DepartmentWithNoDivisionButDivisionsDisabled_ShouldNotHaveDepartmentIdError()
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                null,
                Guid.NewGuid(),
                null,
                null,
                null))
            .Generate();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(false);
        var createUserRequestValidator = new CreateUserRequestValidator(currentUserServiceMock.Object);

        //Act
        var actualResult = createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
    }

    [Fact]
    public void Validation_DepartmentIsNullButDivisionsDisabled_ShouldNotHaveDepartmentIdError()
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                null,
                null,
                null,
                null,
                new Guid()))
            .Generate();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(false);
        var createUserRequestValidator = new CreateUserRequestValidator(currentUserServiceMock.Object);

        //Act
        var actualResult = createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
    }

    [Fact]
    public void Validation_ServiceAreaWithoutDepartmentButDivisionsDisabled_ShouldServiceIdHaveError()
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                null,
                null,
                new Guid(),
                null,
                new Guid()))
            .Generate();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(false);
        var createUserRequestValidator = new CreateUserRequestValidator(currentUserServiceMock.Object);

        //Act
        var actualResult = createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
    }

    [Fact]
    public void Validation_AllRequiredValuesAreNullButDivisionsDisabled_ShouldAllRequiredFieldsHaveError()
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null,
                null))
            .Generate();
        var currentUserServiceMock = new Mock<ICurrentUserService>();
        currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(false);
        var createUserRequestValidator = new CreateUserRequestValidator(currentUserServiceMock.Object);

        //Act
        var actualResult = createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
    }
}

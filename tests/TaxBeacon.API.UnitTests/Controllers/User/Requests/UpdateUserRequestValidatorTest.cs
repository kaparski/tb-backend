using Bogus;
using FluentValidation.TestHelper;
using Moq;
using TaxBeacon.API.Controllers.Users.Requests;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.UnitTests.Controllers.User.Requests;

public class UpdateUserRequestValidatorTest
{
    private readonly UpdateUserRequestValidator _updateUserRequestValidator;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public UpdateUserRequestValidatorTest()
    {
        _currentUserServiceMock = new();
        _updateUserRequestValidator = new UpdateUserRequestValidator(_currentUserServiceMock.Object);
    }

    [Fact]
    public void Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidFirstName_ShouldHaveErrors(string firstName)
    {
        //Arrange
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(
                firstName,
                f.Name.FirstName(),
                f.Name.LastName(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
    }

    [Fact]
    public void Validation_LongFirstName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var firstName = new Faker().Random.String2(115);
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(
                firstName,
                f.Name.FirstName(),
                f.Name.LastName(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.FirstName)
            .WithErrorMessage("The first name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidLegalName_ShouldHaveErrors(string legalName)
    {
        //Arrange
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(
                f.Name.FirstName(),
                legalName,
                f.Name.LastName(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
    }

    [Fact]
    public void Validation_LongLegalName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var legalName = new Faker().Random.String2(115);
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(
                f.Name.FirstName(),
                legalName,
                f.Name.LastName(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult
            .ShouldHaveValidationErrorFor(r => r.LegalName)
            .WithErrorMessage("Legal name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidLastName_ShouldHaveErrors(string lastName)
    {
        //Arrange
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                lastName,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
    }

    [Fact]
    public void Validation_LongLastName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var lastName = new Faker().Random.String2(115);
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                lastName,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.LastName)
            .WithErrorMessage("The last name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
    }

    [Fact]
    public void Validation_DepartmentWithNoDivisionAndDivisionsEnabled_ShouldHaveDepartmentError()
    {
        //Arrange
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                null,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldHaveValidationErrorFor(r => r.DepartmentId)
            .WithErrorMessage("Cannot set a department without a division.");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
    }

    [Fact]
    public void Validation_DepartmentWithNoDivisionAndDivisionsDisabled_ShouldHaveNoErrors()
    {
        //Arrange
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                null,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(false);

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
    }

    [Fact]
    public void Validation_ServiceAreaWithNoDepartmentAndDivisionsEnabled_ShouldHaveServiceAreaError()
    {
        //Arrange
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                Guid.NewGuid(),
                null,
                Guid.NewGuid(),
                null,
                Guid.NewGuid()))
            .Generate();

        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldHaveValidationErrorFor(r => r.ServiceAreaId)
            .WithErrorMessage("Cannot set a service area without a department.");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.JobTitleId);
    }

    [Fact]
    public void Validation_JobTitleWithNoDepartmentAndDivisionsEnabled_ShouldHaveJobTitleError()
    {
        //Arrange
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                Guid.NewGuid(),
                null,
                null,
                Guid.NewGuid(),
                Guid.NewGuid()))
            .Generate();

        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DivisionId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.DepartmentId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.ServiceAreaId);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TeamId);
        actualResult.ShouldHaveValidationErrorFor(r => r.JobTitleId)
            .WithErrorMessage("Cannot set a job title without a department.");
    }
}

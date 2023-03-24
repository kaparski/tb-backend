using Bogus;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Users.Requests;

namespace TaxBeacon.API.UnitTests.Controllers.User.Requests;

public class UpdateUserRequestValidatorTest
{
    private readonly UpdateUserRequestValidator _updateUserRequestValidator;

    public UpdateUserRequestValidatorTest() => _updateUserRequestValidator = new UpdateUserRequestValidator();

    [Fact]
    public void Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(f.Name.FirstName(), f.Name.LastName()))
            .Generate();

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidFirstName_ShouldHaveErrors(string firstName)
    {
        //Arrange
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(firstName, f.Name.LastName()))
            .Generate();

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Fact]
    public void Validation_LongFirstName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var firstName = new Faker().Random.String2(115);
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(firstName, f.Name.LastName()))
            .Generate();

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.FirstName)
            .WithErrorMessage("The first name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidLastName_ShouldHaveErrors(string lastName)
    {
        //Arrange
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(f.Name.FirstName(), lastName))
            .Generate();

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
    }

    [Fact]
    public void Validation_LongLastName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var lastName = new Faker().Random.String2(115);
        var updateUserRequest = new Faker<UpdateUserRequest>()
            .CustomInstantiator(f => new UpdateUserRequest(f.Name.FirstName(), lastName))
            .Generate();

        //Act
        var actualResult = _updateUserRequestValidator.TestValidate(updateUserRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.LastName)
            .WithErrorMessage("The last name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
    }
}

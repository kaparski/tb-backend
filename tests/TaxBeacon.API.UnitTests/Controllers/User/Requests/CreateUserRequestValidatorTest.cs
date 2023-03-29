using Bogus;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Users.Requests;

namespace TaxBeacon.API.UnitTests.Controllers.User.Requests;

public class CreateUserRequestValidatorTest
{
    private readonly CreateUserRequestValidator _createUserRequestValidator;

    public CreateUserRequestValidatorTest() => _createUserRequestValidator = new CreateUserRequestValidator();

    [Fact]
    public void Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(f.Name.FirstName(), f.Name.LastName(), f.Internet.Email()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
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
            .CustomInstantiator(f => new CreateUserRequest(f.Name.FirstName(), f.Name.LastName(), email))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Fact]
    public void Validation_LongEmail_ShouldHaveMaxLengthError()
    {
        //Arrange
        var email = $"{new Faker().Random.String2(200)}@email.com";
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(f.Name.FirstName(), f.Name.LastName(), email))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Email)
            .WithErrorMessage("The email must contain no more than 64 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidFirstName_ShouldHaveErrors(string firstName)
    {
        //Arrange
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(firstName, f.Name.LastName(), f.Internet.Email()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.FirstName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.LastName);
    }

    [Fact]
    public void Validation_LongFirstName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var firstName = new Faker().Random.String2(115);
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(firstName, f.Name.LastName(), f.Internet.Email()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.FirstName)
            .WithErrorMessage("The first name must contain no more than 100 characters");
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
            .CustomInstantiator(f => new CreateUserRequest(f.Name.FirstName(), lastName, f.Internet.Email()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.LastName);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
    }

    [Fact]
    public void Validation_LongLastName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var lastName = new Faker().Random.String2(115);
        var createUserRequest = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(f.Name.FirstName(), lastName, f.Internet.Email()))
            .Generate();

        //Act
        var actualResult = _createUserRequestValidator.TestValidate(createUserRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.LastName)
            .WithErrorMessage("The last name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.FirstName);
    }
}

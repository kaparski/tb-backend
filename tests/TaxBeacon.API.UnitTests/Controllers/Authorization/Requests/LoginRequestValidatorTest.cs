using Bogus;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Authorization.Requests;

namespace TaxBeacon.API.UnitTests.Controllers.Authorization.Requests;

public class LoginRequestValidatorTest
{
    private readonly LoginRequestValidator _loginRequestValidator;

    public LoginRequestValidatorTest() => _loginRequestValidator = new LoginRequestValidator();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("1")]
    public void Validation_InvalidEmail_ShouldHaveErrors(string email)
    {
        //Arrange
        var loginRequest = new LoginRequest(email);

        //Act
        var actualResult = _loginRequestValidator.TestValidate(loginRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Email);
    }

    [Fact]
    public void Validation_ValidEmail_ShouldHaveNoErrors()
    {
        //Arrange
        var loginRequest = new LoginRequest(new Faker().Internet.Email());

        //Act
        var actualResult = _loginRequestValidator.TestValidate(loginRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Email);
    }

    [Fact]
    public void Validation_LongEmail_ShouldHaveMaxLengthError()
    {
        //Arrange
        var email = $"{new Faker().Random.String2(200)}@email.com";
        var loginRequest = new LoginRequest(email);

        //Act
        var actualResult = _loginRequestValidator.TestValidate(loginRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Email)
            .WithErrorMessage("The email must contain no more than 150 characters");
    }
}

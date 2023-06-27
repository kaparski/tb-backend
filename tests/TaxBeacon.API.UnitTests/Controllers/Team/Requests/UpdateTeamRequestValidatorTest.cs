using Bogus;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Teams.Requests;

namespace TaxBeacon.API.UnitTests.Controllers.Team.Requests;

public class UpdateTeamRequestValidatorTest
{
    private readonly UpdateTeamRequestValidator _updateTeamRequestValidator;
    public UpdateTeamRequestValidatorTest() => _updateTeamRequestValidator = new UpdateTeamRequestValidator();

    [Fact]
    public void Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var updateTeamRequest = new Faker<UpdateTeamRequest>()
            .CustomInstantiator(f => new UpdateTeamRequest(f.Name.FirstName(), f.Name.JobDescriptor()))
            .Generate();

        //Act
        var actualResult = _updateTeamRequestValidator.TestValidate(updateTeamRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidName_ShouldHaveErrors(string name)
    {
        //Arrange
        var updateTeamRequest = new Faker<UpdateTeamRequest>()
            .CustomInstantiator(f => new UpdateTeamRequest(name, f.Name.JobDescriptor()))
            .Generate();

        //Act
        var actualResult = _updateTeamRequestValidator.TestValidate(updateTeamRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public void Validation_LongName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var name = new Faker().Random.String2(115);
        var updateTeamRequest = new Faker<UpdateTeamRequest>()
            .CustomInstantiator(f => new UpdateTeamRequest(name, f.Name.JobDescriptor()))
            .Generate();

        //Act
        var actualResult = _updateTeamRequestValidator.TestValidate(updateTeamRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("The team name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public void Validation_LongDescription_ShouldHaveMaxLengthError()
    {
        //Arrange
        var description = new Faker().Random.String2(270);
        var updateTeamRequest = new Faker<UpdateTeamRequest>()
            .CustomInstantiator(f => new UpdateTeamRequest(f.Name.FirstName(), description))
            .Generate();

        //Act
        var actualResult = _updateTeamRequestValidator.TestValidate(updateTeamRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Description)
            .WithErrorMessage("The team description must contain no more than 200 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
    }
}
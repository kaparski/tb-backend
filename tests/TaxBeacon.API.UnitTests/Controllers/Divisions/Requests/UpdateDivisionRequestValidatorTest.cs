using Bogus;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Divisions.Requests;

namespace TaxBeacon.API.UnitTests.Controllers.Divisions.Requests;

public class UpdateDivisionRequestValidatorTest
{
    private readonly UpdateDivisionRequestValidator _updateDivisionRequestValidator;
    public UpdateDivisionRequestValidatorTest() => _updateDivisionRequestValidator = new UpdateDivisionRequestValidator();

    [Fact]
    public void Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var updateDivisionRequest = new Faker<UpdateDivisionRequest>()
            .CustomInstantiator(f => new UpdateDivisionRequest(f.Name.FirstName(), f.Name.JobDescriptor(), new List<Guid>()))
            .Generate();

        //Act
        var actualResult = _updateDivisionRequestValidator.TestValidate(updateDivisionRequest);

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
        var updateDivisionRequest = new Faker<UpdateDivisionRequest>()
            .CustomInstantiator(f => new UpdateDivisionRequest(name, f.Name.JobDescriptor(), new List<Guid>()))
            .Generate();

        //Act
        var actualResult = _updateDivisionRequestValidator.TestValidate(updateDivisionRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public void Validation_LongName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var name = new Faker().Random.String2(115);
        var updateDivisionRequest = new Faker<UpdateDivisionRequest>()
            .CustomInstantiator(f => new UpdateDivisionRequest(name, f.Name.JobDescriptor(), new List<Guid>()))
            .Generate();

        //Act
        var actualResult = _updateDivisionRequestValidator.TestValidate(updateDivisionRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("The division name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public void Validation_LongDescription_ShouldHaveMaxLengthError()
    {
        //Arrange
        var description = new Faker().Random.String2(270);
        var updateDivisionRequest = new Faker<UpdateDivisionRequest>()
            .CustomInstantiator(f => new UpdateDivisionRequest(f.Name.FirstName(), description, new List<Guid>()))
            .Generate();

        //Act
        var actualResult = _updateDivisionRequestValidator.TestValidate(updateDivisionRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Description)
            .WithErrorMessage("The division description must contain no more than 200 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
    }
}
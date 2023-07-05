using Bogus;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Programs.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Administration;

namespace TaxBeacon.API.UnitTests.Controllers.Programs.Requests;

public class CreateProgramRequestValidatorTests
{
    private readonly CreateProgramRequestValidator _createProgramRequestValidator;

    public CreateProgramRequestValidatorTests() => _createProgramRequestValidator = new CreateProgramRequestValidator();

    [Fact]
    public void Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var createProgramRequest = new Faker<CreateProgramRequest>()
            .CustomInstantiator(f => new CreateProgramRequest(
                f.Lorem.Word(),
                f.Company.CompanyName(),
                f.Lorem.Word(),
                f.Lorem.Word(),
                f.Internet.Url(),
                f.PickRandom<Jurisdiction>(),
                f.Address.State(),
                f.Address.Country(),
                f.Address.City(),
                f.Lorem.Word(),
                f.Lorem.Word(),
                f.Date.Past(),
                f.Date.Future()))
            .Generate();

        //Act
        var actualResult = _createProgramRequestValidator.TestValidate(createProgramRequest);

        //Assert
        using (new AssertionScope())
        {
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Reference);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Overview);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalAuthority);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Agency);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Jurisdiction);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.State);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.County);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.City);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.IncentivesArea);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.IncentivesType);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.StartDateTimeUtc);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.EndDateTimeUtc);
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidName_ShouldHaveErrors(string name)
    {
        //Arrange
        var createProgramRequest = new Faker<CreateProgramRequest>()
            .CustomInstantiator(f => new CreateProgramRequest(
                name,
                f.Company.CompanyName(),
                f.Lorem.Word(),
                f.Lorem.Word(),
                f.Internet.Url(),
                f.PickRandom<Jurisdiction>(),
                f.Address.State(),
                f.Address.Country(),
                f.Address.City(),
                f.Lorem.Word(),
                f.Lorem.Word(),
                f.Date.Past(),
                f.Date.Future()))
            .Generate();

        //Act
        var actualResult = _createProgramRequestValidator.TestValidate(createProgramRequest);

        //Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(r => r.Name);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Reference);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Overview);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalAuthority);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Agency);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Jurisdiction);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.State);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.County);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.City);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.IncentivesArea);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.IncentivesType);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.StartDateTimeUtc);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.EndDateTimeUtc);
        }
    }

    [Fact]
    public void Validation_LongName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var name = new Faker().Random.String2(115);
        var createProgramRequest = new Faker<CreateProgramRequest>()
            .CustomInstantiator(f => new CreateProgramRequest(
                name,
                f.Company.CompanyName(),
                f.Lorem.Word(),
                f.Lorem.Word(),
                f.Internet.Url(),
                f.PickRandom<Jurisdiction>(),
                f.Address.State(),
                f.Address.Country(),
                f.Address.City(),
                f.Lorem.Word(),
                f.Lorem.Word(),
                f.Date.Past(),
                f.Date.Future()))
            .Generate();

        //Act
        var actualResult = _createProgramRequestValidator.TestValidate(createProgramRequest);

        //Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(r => r.Name)
                .WithErrorMessage("The program name must contain no more than 100 characters");
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Reference);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Overview);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalAuthority);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Agency);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Jurisdiction);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.State);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.County);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.City);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.IncentivesArea);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.IncentivesType);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.StartDateTimeUtc);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.EndDateTimeUtc);
        }
    }

    [Fact]
    public void Validation_StartDateGreaterThanEndDate_ShouldHaveError()
    {
        //Arrange
        var createProgramRequest = new Faker<CreateProgramRequest>()
            .CustomInstantiator(f => new CreateProgramRequest(
                f.Lorem.Word(),
                f.Company.CompanyName(),
                f.Lorem.Word(),
                f.Lorem.Word(),
                f.Internet.Url(),
                f.PickRandom<Jurisdiction>(),
                f.Address.State(),
                f.Address.Country(),
                f.Address.City(),
                f.Lorem.Word(),
                f.Lorem.Word(),
                f.Date.Future(),
                f.Date.Past()))
            .Generate();

        //Act
        var actualResult = _createProgramRequestValidator.TestValidate(createProgramRequest);

        //Assert
        using (new AssertionScope())
        {
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Reference);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Overview);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.LegalAuthority);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Agency);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.Jurisdiction);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.State);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.County);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.City);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.IncentivesArea);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.IncentivesType);
            actualResult.ShouldNotHaveValidationErrorFor(r => r.StartDateTimeUtc);
            actualResult.ShouldHaveValidationErrorFor(r => r.EndDateTimeUtc)
                .WithErrorMessage("The program end date must be greater than or equal to the program start date");
        }
    }
}

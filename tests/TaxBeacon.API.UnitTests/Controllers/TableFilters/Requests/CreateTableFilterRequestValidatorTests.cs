using Bogus;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.TableFilters.Requests;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.UnitTests.Controllers.TableFilters.Requests;

public class CreateTableFilterRequestValidatorTests
{
    private readonly CreateTableFilterRequestValidator _createTableFilterRequestValidator;

    public CreateTableFilterRequestValidatorTests() =>
        _createTableFilterRequestValidator = new CreateTableFilterRequestValidator();

    [Fact]
    public void Validation_ValidRequest_ShouldHaveNoErrors()
    {
        // Arrange
        var createTableFilterRequest = new Faker<CreateTableFilterRequest>()
            .CustomInstantiator(f =>
                new CreateTableFilterRequest(f.Hacker.Noun(), f.Lorem.Text(), f.PickRandom<EntityType>()))
            .Generate();

        // Act
        var actualResult = _createTableFilterRequestValidator.TestValidate(createTableFilterRequest);

        // Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Configuration);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TableType);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidName_ShouldHaveErrors(string name)
    {
        // Arrange
        var createTableFilterRequest = new Faker<CreateTableFilterRequest>()
            .CustomInstantiator(f =>
                new CreateTableFilterRequest(name, f.Lorem.Text(), f.PickRandom<EntityType>()))
            .Generate();

        // Act
        var actualResult = _createTableFilterRequestValidator.TestValidate(createTableFilterRequest);

        // Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Configuration);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TableType);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Validation_InvalidConfiguration_ShouldHaveErrors(string configuration)
    {
        // Arrange
        var createTableFilterRequest = new Faker<CreateTableFilterRequest>()
            .CustomInstantiator(f =>
                new CreateTableFilterRequest(f.Hacker.Noun(), configuration, f.PickRandom<EntityType>()))
            .Generate();

        // Act
        var actualResult = _createTableFilterRequestValidator.TestValidate(createTableFilterRequest);

        // Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldHaveValidationErrorFor(r => r.Configuration);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TableType);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(100)]
    [InlineData(200)]
    public void Validation_InvalidTableType_ShouldHaveErrors(int tableType)
    {
        // Arrange
        var createTableFilterRequest = new Faker<CreateTableFilterRequest>()
            .CustomInstantiator(f =>
                new CreateTableFilterRequest(f.Hacker.Noun(), f.Lorem.Text(), (EntityType)tableType))
            .Generate();

        // Act
        var actualResult = _createTableFilterRequestValidator.TestValidate(createTableFilterRequest);

        // Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Configuration);
        actualResult.ShouldHaveValidationErrorFor(r => r.TableType);
    }

    [Fact]
    public void Validation_LongName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var name = new Faker().Random.String2(200);
        var createTableFilterRequest = new Faker<CreateTableFilterRequest>()
            .CustomInstantiator(f =>
                new CreateTableFilterRequest(name, f.Lorem.Text(), f.PickRandom<EntityType>()))
            .Generate();

        //Act
        var actualResult = _createTableFilterRequestValidator.TestValidate(createTableFilterRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("The name must contain no more than 50 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Configuration);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.TableType);
    }
}

using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Locations.Requests;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.UnitTests.Controllers.Locations.Requests;

public class ExportLocationsValidatorTests
{
    private readonly ExportLocationsRequestValidator _exportLocationsRequestValidator;

    public ExportLocationsValidatorTests() =>
        _exportLocationsRequestValidator = new ExportLocationsRequestValidator();

    [Fact]
    public void Validation_InvalidFileType_ShouldHaveErrors()
    {
        //Arrange
        var exportRequest = new ExportLocationsRequest((FileType)101, "America/New_York");

        //Act
        var actualResult = _exportLocationsRequestValidator.TestValidate(exportRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.FileType);
    }

    [Fact]
    public void Validation_InvalidTimeZone_ShouldHaveErrors()
    {
        //Arrange
        var exportRequest = new ExportLocationsRequest(FileType.Csv, "invalid timezone");

        //Act
        var actualResult = _exportLocationsRequestValidator.TestValidate(exportRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.IanaTimeZone);
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public void Validation_ValidRequest_ShouldHaveNoErrors(FileType fileType)
    {
        //Arrange
        var exportRequest = new ExportLocationsRequest(fileType, "America/New_York");

        //Act
        var actualResult = _exportLocationsRequestValidator.TestValidate(exportRequest);

        //Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
    }
}
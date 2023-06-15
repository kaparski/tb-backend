using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Accounts.Requests;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.UnitTests.Controllers.Accounts.Requests;

public sealed class ExportAccountsRequestValidatorTests
{
    private readonly ExportAccountsRequestValidator _exportAccountsRequestValidator;

    public ExportAccountsRequestValidatorTests() =>
        _exportAccountsRequestValidator = new ExportAccountsRequestValidator();

    [Fact]
    public void Validation_InvalidFileType_ShouldHaveErrors()
    {
        //Arrange
        var exportRequest = new ExportAccountsRequest((FileType)101, "America/New_York");

        //Act
        var actualResult = _exportAccountsRequestValidator.TestValidate(exportRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.FileType);
    }

    [Fact]
    public void Validation_InvalidTimeZone_ShouldHaveErrors()
    {
        //Arrange
        var exportRequest = new ExportAccountsRequest(FileType.Csv, "invalid timezone");

        //Act
        var actualResult = _exportAccountsRequestValidator.TestValidate(exportRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.IanaTimeZone);
    }

    [Fact]
    public void Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var exportRequest = new ExportAccountsRequest(FileType.Csv, "America/New_York");

        //Act
        var actualResult = _exportAccountsRequestValidator.TestValidate(exportRequest);

        //Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
    }
}

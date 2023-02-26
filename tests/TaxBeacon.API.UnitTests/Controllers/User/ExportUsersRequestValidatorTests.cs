using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Users.Requests;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.UnitTests.Controllers.User
{
    public sealed class ExportUsersRequestValidatorTests
    {
        private readonly ExportUsersRequestValidator _exportUsersRequestValidator;

        public ExportUsersRequestValidatorTests()
            => _exportUsersRequestValidator = new ExportUsersRequestValidator();

        [Fact]
        public void Validation_InvalidFileType_ShouldHaveErrors()
        {
            //Arrange
            var exportRequest = new ExportUsersRequest((FileType)101, "America/New_York");

            //Act
            var actualResult = _exportUsersRequestValidator.TestValidate(exportRequest);

            //Assert
            actualResult.ShouldHaveValidationErrorFor(r => r.FileType);
        }

        [Fact]
        public void Validation_InvalidTimeZone_ShouldHaveErrors()
        {
            //Arrange
            var exportRequest = new ExportUsersRequest(FileType.Csv, "invalid timezone");

            //Act
            var actualResult = _exportUsersRequestValidator.TestValidate(exportRequest);

            //Assert
            actualResult.ShouldHaveValidationErrorFor(r => r.IanaTimeZone);
        }

        [Fact]
        public void Validation_InputDateIsValid_ShouldHaveNoErrors()
        {
            //Arrange
            var exportRequest = new ExportUsersRequest(FileType.Csv, "America/New_York");

            //Act
            var actualResult = _exportUsersRequestValidator.TestValidate(exportRequest);

            //Assert
            actualResult.ShouldNotHaveAnyValidationErrors();
        }
    }
}

using FileSignatures;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Moq;
using TaxBeacon.API.Shared.Validators;

namespace TaxBeacon.API.UnitTests.Shared.Validators;

public class TestFileFormat: FileFormat
{
    public TestFileFormat(byte[] signature, string mediaType, string extension) : base(signature, mediaType, extension)
    { }
}

public class FormFileValidatorTests
{
    private readonly Mock<IFileFormatInspector> _inspectorMock = new();
    private readonly string[] _allowedMimeTypes =
    {
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };
    private readonly FormFileValidator _validator;

    public FormFileValidatorTests() =>
        _validator = new FormFileValidator(
            _inspectorMock.Object, 100_000, _allowedMimeTypes.ToHashSet());

    [Fact]
    public void Validate_ValidRequest_ReturnsNoErrors()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        fileMock.Setup(f => f.Length).Returns(100);
        fileMock.Setup(f => f.ContentType).Returns(_allowedMimeTypes[0]);

        _inspectorMock
            .Setup(i => i.DetermineFileFormat(It.IsAny<Stream>()))
            .Returns(new TestFileFormat(Array.Empty<byte>(), _allowedMimeTypes[0], ".xlsx"));

        // Act
        var result = _validator.TestValidate(fileMock.Object);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ContentLengthIsGreaterThatAllowed_ReturnsError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        fileMock.Setup(f => f.Length).Returns(200_000);
        fileMock.Setup(f => f.ContentType).Returns(_allowedMimeTypes[0]);

        _inspectorMock
            .Setup(i => i.DetermineFileFormat(It.IsAny<Stream>()))
            .Returns(new TestFileFormat(Array.Empty<byte>(), _allowedMimeTypes[0], ".xlsx"));

        // Act
        var result = _validator.TestValidate(fileMock.Object);

        // Assert
        result.ShouldHaveValidationErrorFor(file => file.Length);
        result.ShouldNotHaveValidationErrorFor(file => file.ContentType);
        result.ShouldNotHaveValidationErrorFor(file => file);
    }

    [Fact]
    public void Validate_ContentTypeIsNotAllowed_ReturnsError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        fileMock.Setup(f => f.Length).Returns(50_000);
        fileMock.Setup(f => f.ContentType).Returns("application/zip");

        _inspectorMock
            .Setup(i => i.DetermineFileFormat(It.IsAny<Stream>()))
            .Returns(new TestFileFormat(Array.Empty<byte>(), "application/zip", ".zip"));

        // Act
        var result = _validator.TestValidate(fileMock.Object);

        // Assert
        result.ShouldNotHaveValidationErrorFor(file => file.Length);
        result.ShouldHaveValidationErrorFor(file => file);
        result.ShouldHaveValidationErrorFor(file => file.ContentType);
    }

    [Fact]
    public void Validate_DetermineFileFormatReturnsNotAllowedFileFormat_ReturnsError()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Returns(new MemoryStream());
        fileMock.Setup(f => f.Length).Returns(50_000);
        fileMock.Setup(f => f.ContentType).Returns(_allowedMimeTypes[0]);

        _inspectorMock
            .Setup(i => i.DetermineFileFormat(It.IsAny<Stream>()))
            .Returns(new TestFileFormat(Array.Empty<byte>(), "application/zip", ".zip"));

        // Act
        var result = _validator.TestValidate(fileMock.Object);

        // Assert
        result.ShouldNotHaveValidationErrorFor(file => file.Length);
        result.ShouldNotHaveValidationErrorFor(file => file.ContentType);
        result.ShouldHaveValidationErrorFor(file => file);
    }
}

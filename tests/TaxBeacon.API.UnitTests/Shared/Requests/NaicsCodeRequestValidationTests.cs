using FluentValidation.TestHelper;
using Moq;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.API.Shared.Requests;

namespace TaxBeacon.API.UnitTests.Shared.Requests;
public sealed class NaicsCodeRequestValidationTests
{
    private readonly Mock<INaicsService> _naicsServiceMock;
    private readonly NaicsCodeRequestValidation _naicsCodeRequestValidation;
    public NaicsCodeRequestValidationTests()
    {
        _naicsServiceMock = new();
        _naicsCodeRequestValidation = new(_naicsServiceMock.Object);
    }

    [Fact]
    public async Task ValidateAsync_ValidRequest_ShouldHaveNoErrors()
    {
        // Arrange
        var naicsModel = new Mock<INaicsCodeRequest>();
        naicsModel.Setup(x => x.PrimaryNaicsCode).Returns(Random.Shared.Next(100000, 999999));
        _naicsServiceMock.Setup(x => x.IsNaicsValidAsync(It.IsAny<int>(), default)).ReturnsAsync(true);
        // Act
        var result = await _naicsCodeRequestValidation.TestValidateAsync(naicsModel.Object);
        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task ValidateAsync_InvalidRequestNaicsDoesntExist_ReturnsErrors()
    {
        // Arrange
        var naicsModel = new Mock<INaicsCodeRequest>();
        naicsModel.Setup(x => x.PrimaryNaicsCode).Returns(Random.Shared.Next(100000, 999999));
        _naicsServiceMock.Setup(x => x.IsNaicsValidAsync(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(false);
        var naicsCodeRequestValidation = new NaicsCodeRequestValidation(_naicsServiceMock.Object);
        // Act
        var result = await naicsCodeRequestValidation.TestValidateAsync(naicsModel.Object, cancellationToken: CancellationToken.None);
        // Assert
        _naicsServiceMock.Verify(x => x.IsNaicsValidAsync(naicsModel.Object.PrimaryNaicsCode, CancellationToken.None));
        result.ShouldHaveValidationErrorFor(nameof(INaicsCodeRequest.PrimaryNaicsCode)).WithErrorMessage("This NAICS code doesn't exist");
    }

    [Fact]
    public async Task ValidateAsync_InvalidRequestNaicsNumberIsInvalid_ReturnsErrors()
    {
        // Arrange
        var naicsModel = new Mock<INaicsCodeRequest>();
        naicsModel.Setup(x => x.PrimaryNaicsCode).Returns(Random.Shared.Next(1000, 30000));
        _naicsServiceMock.Setup(x => x.IsNaicsValidAsync(It.IsAny<int>(), CancellationToken.None)).ReturnsAsync(false);
        var naicsCodeRequestValidation = new NaicsCodeRequestValidation(_naicsServiceMock.Object);
        // Act
        var result = await naicsCodeRequestValidation.TestValidateAsync(naicsModel.Object, cancellationToken: CancellationToken.None);
        // Assert
        _naicsServiceMock.Verify(x => x.IsNaicsValidAsync(naicsModel.Object.PrimaryNaicsCode, CancellationToken.None), Times.Never);
        result.ShouldHaveValidationErrorFor(nameof(INaicsCodeRequest.PrimaryNaicsCode)).WithErrorMessage("Please enter a six digit number");
    }
}

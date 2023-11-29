using FluentAssertions;
using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Templates.Requests;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.UnitTests.Controllers.Templates.Requests;

public sealed class GetImportTemplatesRequestValidatorTests
{
    private readonly GetImportTemplatesRequestValidator _getImportTemplatesRequestValidator = new();

    [Theory]
    [MemberData(nameof(TestData.ValidTemplateTypes), MemberType = typeof(TestData))]
    public async Task GetImportTemplatesRequest_ValidTemplateType_ShouldReturnNoError(TemplateType templateType)
    {
        // Arrange
        var request = new GetImportTemplatesRequest { TemplateType = templateType };

        // Act
        var actualResult = await _getImportTemplatesRequestValidator.TestValidateAsync(request);

        // Assert
        actualResult.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task GetImportTemplatesRequest_TemplateTypeEqualsNone_ShouldHaveErrors()
    {
        // Arrange
        var request = new GetImportTemplatesRequest { TemplateType = TemplateType.None };

        // Act
        var actualResult = await _getImportTemplatesRequestValidator.TestValidateAsync(request);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(s => s.TemplateType);
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    [Fact]
    public async Task GetImportTemplatesRequest_TemplateTypeNotInEnum_ShouldHaveErrors()
    {
        // Arrange
        var request = new GetImportTemplatesRequest { TemplateType = (TemplateType)int.MaxValue };

        // Act
        var actualResult = await _getImportTemplatesRequestValidator.TestValidateAsync(request);

        // Assert
        using (new AssertionScope())
        {
            actualResult.ShouldHaveValidationErrorFor(s => s.TemplateType);
            actualResult.Errors.Count.Should().Be(1);
        }
    }

    private static class TestData
    {
        public static IEnumerable<object[]> ValidTemplateTypes = Enum.GetValues<TemplateType>()
            .Where(t => t != TemplateType.None)
            .Select(t => new object[] { t });
    }
}

using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Tenants.Requests;

namespace TaxBeacon.API.UnitTests.Controllers.Tenant.Requests;

public class UpdateTenantRequestValidatorTests
{
    private readonly UpdateTenantRequestValidator _updateTenantRequestValidator = new();

    [Theory]
    [InlineData("A")]
    [InlineData("CTI")]
    [InlineData("wJCdwKHtjsdGADIkUzzQdhWgmJYCOPrKBRWuyhksngXssEyMYxnKFlUerGueRWxGXIVBdAAiIckspHqjedCAYZgdXkSbhKTVOCEo")] // Length 100
    public void UpdateTenantRequest_ValidName_ShouldReturnNoError(string name)
    {
        // Arrange
        var request = new UpdateTenantRequest(name);

        // Act
        var actualResult = _updateTenantRequestValidator.TestValidate(request);

        // Assert
        actualResult.ShouldNotHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("hqFfPYvYUoiMDmxDakvMGGxIoNambUfwxAfvDUdgzGzgJKmfbZmwKqsWYbccENwgqXtwlBoKRiqHdcZxrkBuBYPUcjKBGUMIdaGOB")] // Length 101
    public void UpdateTenantRequest_InvalidName_ShouldReturnError(string name)
    {
        // Arrange
        var request = new UpdateTenantRequest(name);

        // Act
        var actualResult = _updateTenantRequestValidator.TestValidate(request);

        // Assert
        actualResult.ShouldHaveValidationErrorFor(x => x.Name);
    }
}

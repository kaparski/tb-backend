using FluentAssertions.Execution;
using FluentValidation.TestHelper;
using TaxBeacon.API.Controllers.Tenants.Requests;

namespace TaxBeacon.API.UnitTests.Controllers.Tenant.Requests;

public class TenantActivityHistoryRequestValidatorTest
{
    private readonly TenantActivityHistoryRequestValidator _tenantActivityHistoryRequestValidator;

    public TenantActivityHistoryRequestValidatorTest() => _tenantActivityHistoryRequestValidator = new TenantActivityHistoryRequestValidator();

    [Theory]
    [InlineData(0, 5)]
    [InlineData(-1, 5)]
    public void TenantActivityHistoryRequest_InvalidPage_ShouldReturnErrorForPage(int page, int pageSize)
    {
        //Arrange
        var tenantActivityHistoryRequest = new TenantActivityHistoryRequest(page, pageSize);

        //Act
        var actualResult = _tenantActivityHistoryRequestValidator.TestValidate(tenantActivityHistoryRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(x => x.Page);
        actualResult.ShouldNotHaveValidationErrorFor(x => x.PageSize);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, -1)]
    public void TenantActivityHistoryRequest_InvalidPageSize_ShouldReturnErrorForPageSize(int page, int pageSize)
    {
        //Arrange
        var tenantActivityHistoryRequest = new TenantActivityHistoryRequest(page, pageSize);

        //Act
        var actualResult = _tenantActivityHistoryRequestValidator.TestValidate(tenantActivityHistoryRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(x => x.Page);
        actualResult.ShouldHaveValidationErrorFor(x => x.PageSize);
    }
}

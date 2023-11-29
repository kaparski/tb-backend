using Moq;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportReferralPartnersAsync_ReturnsReferralPartnerExportDto(FileType fileType)
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        var currentTenantId = tenants[0].Id;

        var referrals = TestData.ReferralsFaker
            .RuleFor(a => a.Account, _ => TestData.AccountsFaker
                .RuleFor(a => a.TenantId, _ => currentTenantId)
                .Generate())
            .RuleFor(a => a.TenantId, f => currentTenantId)
            .RuleFor(a => a.State, f => ReferralState.ReferralPartner.Name)
            .Generate(2);

        await _dbContextMock.Referrals.AddRangeAsync(referrals);
        await _dbContextMock.SaveChangesAsync();

        // Act
        await _accountService.ExportReferralPartnersAsync(fileType, default);

        // Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<ReferralPartnerExportDto>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<ReferralPartnerExportDto>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}

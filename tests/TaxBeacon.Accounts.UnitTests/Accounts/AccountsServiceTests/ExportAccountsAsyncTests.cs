using Moq;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportAccountsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var accounts = TestData.AccountsViewFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.AccountsView.AddRangeAsync(accounts);
        await _dbContextMock.SaveChangesAsync();
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        // Act
        _ = await _accountService.ExportAccountsAsync(fileType);

        // Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x => x.Convert(It.IsAny<List<AccountExportDto>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x => x.Convert(It.IsAny<List<AccountExportDto>>()), Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}

using Moq;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.UnitTests.Accounts;
public sealed partial class AccountsServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportClientsProspectsAsync_ReturnsClientProspectExportDto(FileType fileType)
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        var currentTenantId = tenants[0].Id;

        var clients = TestData.ClientsFaker
            .RuleFor(a => a.Account, _ => TestData.AccountsFaker
                .RuleFor(a => a.TenantId, _ => currentTenantId)
                .Generate())
            .RuleFor(a => a.TenantId, f => currentTenantId)
            .RuleFor(a => a.State, f => ClientState.ClientProspect.Name)
            .Generate(2);

        await _dbContextMock.Clients.AddRangeAsync(clients);
        await _dbContextMock.SaveChangesAsync();

        // Act
        await _accountService.ExportClientsProspectsAsync(fileType, default);

        // Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<ClientProspectExportDto>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<ClientProspectExportDto>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}

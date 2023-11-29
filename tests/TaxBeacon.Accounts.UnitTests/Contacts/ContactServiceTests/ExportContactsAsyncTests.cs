using Moq;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.UnitTests.Contacts;
public partial class ContactServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportContactsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var items = TestData.ContactFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate(5);

        await _dbContext.Contacts.AddRangeAsync(items);
        await _dbContext.SaveChangesAsync();

        _currentUserServiceMock
            .SetupGet(service => service.TenantId)
            .Returns(tenant.Id);

        // Act
        _ = await _contactService.ExportContactsAsync(fileType);

        // Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x => x.Convert(It.IsAny<List<ContactExportModel>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x => x.Convert(It.IsAny<List<ContactExportModel>>()), Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}

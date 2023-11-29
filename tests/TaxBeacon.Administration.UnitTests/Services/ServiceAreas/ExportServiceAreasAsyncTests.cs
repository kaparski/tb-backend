using Moq;
using TaxBeacon.Administration.ServiceAreas.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.UnitTests.Services.ServiceAreas;
public partial class ServiceAreaServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportServiceAreasAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var serviceAreas = TestData.TestServiceArea.Generate(5);

        await _dbContextMock.ServiceAreas.AddRangeAsync(serviceAreas);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        //Act
        _ = await _serviceAreaService.ExportServiceAreasAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<ServiceAreaExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<ServiceAreaExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}

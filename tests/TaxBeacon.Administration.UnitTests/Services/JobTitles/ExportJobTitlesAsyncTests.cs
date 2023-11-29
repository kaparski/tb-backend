using Moq;
using TaxBeacon.Administration.JobTitles.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.UnitTests.Services.JobTitles;
public partial class JobTitleServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportJobTitlesAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var serviceAreas = TestData.TestJobTitle.Generate(5);

        await _dbContextMock.JobTitles.AddRangeAsync(serviceAreas);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        //Act
        _ = await _serviceAreaService.ExportJobTitlesAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<JobTitleExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<JobTitleExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}

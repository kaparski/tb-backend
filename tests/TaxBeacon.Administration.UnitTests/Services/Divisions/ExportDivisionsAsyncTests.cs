using Moq;
using TaxBeacon.Administration.Divisions.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.UnitTests.Services.Divisions;
public partial class DivisionsServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportDivisionsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var teams = TestData.TestDivision.Generate(5);

        await _dbContextMock.Divisions.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _divisionsService.ExportDivisionsAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<DivisionExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<DivisionExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}

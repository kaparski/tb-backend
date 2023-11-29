using Moq;
using TaxBeacon.Administration.Programs.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportProgramsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var programs = TestData.ProgramFaker.Generate(5);

        await _dbContextMock.Programs.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _programService.ExportProgramsAsync(fileType);

        //Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x =>
                    x.Convert(It.IsAny<List<ProgramExportModel>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x =>
                    x.Convert(It.IsAny<List<ProgramExportModel>>()), Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}

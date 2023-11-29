using Moq;
using TaxBeacon.Administration.Programs.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportTenantProgramsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var programs = TestData.TenantProgramFaker.Generate(5);

        await _dbContextMock.TenantsPrograms.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _programService.ExportTenantProgramsAsync(fileType);

        //Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x =>
                    x.Convert(It.IsAny<List<TenantProgramExportModel>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x =>
                    x.Convert(It.IsAny<List<TenantProgramExportModel>>()), Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}

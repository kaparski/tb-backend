using Moq;
using TaxBeacon.Administration.Teams.Models;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.UnitTests.Services.Teams;
public partial class TeamServiceTests
{
    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportTeamsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var teams = TestData.TestTeam.Generate(5);

        await _dbContextMock.Teams.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _teamService.ExportTeamsAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<TeamExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<TeamExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }
}

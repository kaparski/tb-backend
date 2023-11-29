using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    [Fact]
    public async Task GetProgramDetailsAsync_ProgramExists_ReturnsProgramDetailsDto()
    {
        // Arrange
        var program = TestData.ProgramFaker.Generate();

        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _programService.GetProgramDetailsAsync(program.Id);

        // Assert
        actualResult.TryPickT0(out var programDetails, out _).Should().BeTrue();
        programDetails.Should().NotBeNull();
    }

    [Fact]
    public async Task GetProgramDetailsAsync_ProgramDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var program = TestData.ProgramFaker.Generate();

        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _programService.GetProgramDetailsAsync(Guid.NewGuid());

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }
}

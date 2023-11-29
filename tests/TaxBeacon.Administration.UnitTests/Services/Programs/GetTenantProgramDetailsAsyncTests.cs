using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    [Fact]
    public async Task GetTenantProgramDetailsAsync_ProgramExists_ReturnsProgramDetailsDto()
    {
        // Arrange
        var program = TestData.TenantProgramFaker.Generate();

        await _dbContextMock.TenantsPrograms.AddAsync(program);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _programService.GetTenantProgramDetailsAsync(program.Program.Id);

        // Assert
        actualResult.TryPickT0(out var programDetails, out _).Should().BeTrue();
        programDetails.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTenantProgramDetailsAsync_ProgramDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var program = TestData.TenantProgramFaker.Generate();

        await _dbContextMock.TenantsPrograms.AddAsync(program);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _programService.GetTenantProgramDetailsAsync(Guid.NewGuid());

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }
}

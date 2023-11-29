using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    [Theory]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task GetProgramActivityHistory_ProgramDoesNotExist_ReturnsNotFound(
        bool isSuperAdmin, bool isUserInTenant)
    {
        // Arrange
        var program = TestData.ProgramFaker.Generate();

        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.IsSuperAdmin).Returns(isSuperAdmin);
        _currentUserServiceMock.Setup(x => x.IsUserInTenant).Returns(isUserInTenant);

        // Act
        var actualResult = await _programService.GetProgramActivityHistoryAsync(Guid.NewGuid());

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }
}

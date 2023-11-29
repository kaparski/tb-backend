using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.Divisions;
public partial class DivisionsServiceTests
{
    [Fact]
    public async Task GetDivisionDetailsAsync_ValidId_ReturnsDivision()
    {
        //Arrange
        TestData.TestDivision.RuleFor(
            x => x.TenantId, _ => TenantId);
        var divisions = TestData.TestDivision.Generate(5);

        await _dbContextMock.Divisions.AddRangeAsync(divisions);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var result = await _divisionsService.GetDivisionDetailsAsync(divisions[0].Id);

        //Assert
        using (new AssertionScope())
        {
            result.TryPickT0(out var divisionDetails, out _).Should().BeTrue();
            divisionDetails.Id.Should().Be(divisions[0].Id);
        }
    }

    [Fact]
    public async Task GetDivisionDetailsAsync_IdNotInDb_ReturnsNotFound()
    {
        //Arrange
        TestData.TestDivision.RuleFor(
            x => x.TenantId, _ => TenantId);
        var divisions = TestData.TestDivision.Generate(5);

        await _dbContextMock.Divisions.AddRangeAsync(divisions);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var result = await _divisionsService.GetDivisionDetailsAsync(new Guid());

        //Assert
        result.TryPickT1(out _, out _).Should().BeTrue();
    }
}

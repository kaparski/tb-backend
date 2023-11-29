using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Naics;
public partial class NaicsServiceTests
{
    [Fact]
    public async Task IsNaicsValid_InValidCode_ReturnsFalse()
    {
        //Arrange
        var n = 11110;
        var invalidCode = 999999;
        var codes = TestData.NaicsCodeFaker.RuleFor(nc => nc.Code, _ => { n += 10; return n; }).Generate(3);
        await _dbContextMock.NaicsCodes.AddRangeAsync(codes);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _naicsService.IsNaicsValidAsync(invalidCode, default);

        // Assert

        using (new AssertionScope())
        {
            actualResult.Should().BeFalse();
        }
    }

    [Fact]
    public async Task IsNaicsValid_ValidCode_ReturnsTrue()
    {
        //Arrange
        var n = 11110;
        var validCode = 111120;
        var codes = TestData.NaicsCodeFaker.RuleFor(nc => nc.Code, _ => { n += 10; return n; }).Generate(3);
        await _dbContextMock.NaicsCodes.AddRangeAsync(codes);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _naicsService.IsNaicsValidAsync(validCode, default);

        // Assert

        using (new AssertionScope())
        {
            actualResult.Should().BeFalse();
        }
    }
}

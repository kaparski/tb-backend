using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Common.Enums.Administration.Activities;

namespace TaxBeacon.Administration.UnitTests.Services.Divisions;
public partial class DivisionsServiceTests
{
    [Fact]
    public async Task UpdateDivisionAsync_DivisionExists_ReturnsUpdatedDivisionAndCapturesActivityLog()
    {
        // Arrange
        var updateDivisionDto = TestData.UpdateDivisionDtoFaker.Generate();
        var division = TestData.TestDivision.Generate();
        await _dbContextMock.Divisions.AddAsync(division);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _divisionsService.UpdateDivisionAsync(division.Id, updateDivisionDto, default);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var divisionDto, out _);
            divisionDto.Should().NotBeNull();
            divisionDto.Id.Should().Be(division.Id);
            divisionDto.Name.Should().Be(updateDivisionDto.Name);

            var actualActivityLog = await _dbContextMock.DivisionActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(DivisionEventType.DivisionUpdatedEvent);
            actualActivityLog?.TenantId.Should().Be(TenantId);
            actualActivityLog?.DivisionId.Should().Be(division.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }
}

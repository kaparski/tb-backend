using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Administration.Activities;
using Moq;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    [Fact]
    public async Task CreateProgramAsync_CreateProgramDto_ReturnsProgramDetailsAndCapturesActivityLog()
    {
        // Arrange
        var createProgramDto = TestData.CreateProgramDtoFaker.Generate();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _programService.CreateProgramAsync(createProgramDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var programDetailsDto, out _);
            programDetailsDto.Should().NotBeNull();
            programDetailsDto.Should().BeEquivalentTo(createProgramDto,
                opt => opt.ExcludingMissingMembers());

            var actualActivityLog = await _dbContextMock.ProgramActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.TenantId.Should().BeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(ProgramEventType.ProgramCreatedEvent);
            actualActivityLog?.ProgramId.Should().Be(programDetailsDto.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(1));
        }
    }

    [Fact]
    public async Task CreateProgramAsync_ProgramWithNameAlreadyExists_ReturnsNameAlreadyExists()
    {
        // Arrange
        var existingProgram = TestData.ProgramFaker.Generate();
        var createProgramDto = TestData.CreateProgramDtoFaker
            .RuleFor(p => p.Name, _ => existingProgram.Name)
            .Generate();

        await _dbContextMock.Programs.AddAsync(existingProgram);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _programService.CreateProgramAsync(createProgramDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }
}

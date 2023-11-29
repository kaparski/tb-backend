using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.Common.Enums.Administration.Activities;
using Moq;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    [Fact]
    public async Task UpdateProgramAsync_ProgramExists_ReturnsUpdatedProgramDetailsAndCapturesActivityLog()
    {
        // Arrange
        var updateProgramDto = TestData.UpdateProgramDtoFaker.Generate();
        var program = TestData.ProgramFaker.Generate();
        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _programService.UpdateProgramAsync(program.Id, updateProgramDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var programDetailsDto, out _);
            programDetailsDto.Should().NotBeNull();
            programDetailsDto.Should().BeEquivalentTo(program,
                opt => opt.ExcludingMissingMembers());

            var actualActivityLog = await _dbContextMock.ProgramActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(ProgramEventType.ProgramUpdatedEvent);
            actualActivityLog?.ProgramId.Should().Be(program.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateProgramAsync_ProgramDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateProgramDto = TestData.UpdateProgramDtoFaker.Generate();
        var program = TestData.ProgramFaker.Generate();
        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _programService.UpdateProgramAsync(Guid.NewGuid(), updateProgramDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT2.Should().BeFalse();
        }
    }

    [Fact]
    public async Task UpdateProgramAsync_ProgramWithNameAlreadyExists_ReturnsNameAlreadyExists()
    {
        // Arrange
        var existingProgram = TestData.ProgramFaker.Generate();
        var updateProgramDto = TestData.UpdateProgramDtoFaker
            .RuleFor(p => p.Name, _ => existingProgram.Name)
            .Generate();

        await _dbContextMock.Programs.AddAsync(existingProgram);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _programService.UpdateProgramAsync(Guid.NewGuid(), updateProgramDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeFalse();
            actualResult.IsT2.Should().BeTrue();
        }
    }
}

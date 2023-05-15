using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models.Programs;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class ProgramServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly ProgramService _programService;

    public ProgramServiceTests()
    {
        Mock<ILogger<ProgramService>> programServiceLoggerMock = new();
        Mock<EntitySaveChangesInterceptor> entitySaveChangesInterceptorMock = new();
        Mock<IDateTimeFormatter> dateTimeFormatterMock = new();
        Mock<IEnumerable<IListToFileConverter>> listToFileConverters = new();

        _csvMock = new();
        _xlsxMock = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(ProgramServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _currentUserServiceMock = new();

        _dateTimeServiceMock = new();

        _programService = new ProgramService(
            programServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            listToFileConverters.Object,
            dateTimeFormatterMock.Object);
    }

    [Fact]
    public async Task GetAllProgramsAsync_QueryIsValidOrderByNameAscending_ReturnsListOfPrograms()
    {
        // Arrange
        var programs = TestData.TestProgram.Generate(10);
        await _dbContextMock.Programs.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 5, OrderBy = "name asc", };

        // Act
        var actualResult = await _programService.GetAllProgramsAsync(query);

        // Arrange
        using (new AssertionScope())
        {
            actualResult.Should().NotBeNull();
            actualResult.Count.Should().Be(10);
            var listOfServiceAreas = actualResult.Query.ToList();
            listOfServiceAreas.Count.Should().Be(5);
            listOfServiceAreas.Select(x => x.Name).Should().BeInAscendingOrder();
        }
    }

    [Fact]
    public async Task GetAllProgramsAsync_QueryIsValidOrderByNameDescending_ReturnsListOfPrograms()
    {
        // Arrange
        var programs = TestData.TestProgram.Generate(10);
        await _dbContextMock.Programs.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 5, OrderBy = "name desc", };

        // Act
        var actualResult = await _programService.GetAllProgramsAsync(query);

        // Arrange
        using (new AssertionScope())
        {
            actualResult.Should().NotBeNull();
            actualResult.Count.Should().Be(10);
            var listOfServiceAreas = actualResult.Query.ToList();
            listOfServiceAreas.Count.Should().Be(5);
            listOfServiceAreas.Select(x => x.Name).Should().BeInDescendingOrder();
        }
    }

    [Fact]
    public async Task GetAllProgramsAsync_PageNumberIsOutOfRange_ReturnsEmptyList()
    {
        // Arrange
        var programs = TestData.TestProgram.Generate(10);
        await _dbContextMock.Programs.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 3, PageSize = 5, OrderBy = "name asc", };

        // Act
        var actualResult = await _programService.GetAllProgramsAsync(query);

        // Arrange
        using (new AssertionScope())
        {
            actualResult.Should().NotBeNull();
            actualResult.Count.Should().Be(10);
            var listOfServiceAreas = actualResult.Query.ToList();
            listOfServiceAreas.Count.Should().Be(0);
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportProgramsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var programs = TestData.TestProgram.Generate(5);

        await _dbContextMock.Programs.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _programService.ExportProgramsAsync(fileType);

        //Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x =>
                    x.Convert(It.IsAny<List<ProgramExportModel>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x =>
                    x.Convert(It.IsAny<List<ProgramExportModel>>()), Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task GetProgramDetailsAsync_ProgramExists_ReturnsProgramDetailsDto()
    {
        // Arrange
        var program = TestData.TestProgram.Generate();

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
        var program = TestData.TestProgram.Generate();

        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _programService.GetProgramDetailsAsync(Guid.NewGuid());

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    private static class TestData
    {
        public static readonly Faker<Program> TestProgram = new Faker<Program>()
            .RuleFor(p => p.Id, f => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Name.FirstName());
    }
}

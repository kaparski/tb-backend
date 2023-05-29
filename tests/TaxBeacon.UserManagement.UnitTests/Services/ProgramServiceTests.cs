using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Services.Program;
using TaxBeacon.UserManagement.Services.Program.Activities;
using TaxBeacon.UserManagement.Services.Program.Activities.Models;
using TaxBeacon.UserManagement.Services.Program.Models;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class ProgramServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly ProgramService _programService;
    private static readonly Guid TenantId = Guid.NewGuid();

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
        _dbContextMock.Tenants.Add(new Tenant()
        {
            Id = TenantId,
            Name = "TestTenant",
        });
        _dbContextMock.SaveChangesAsync();
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TenantId);

        _dateTimeServiceMock = new();

        Mock<IEnumerable<IProgramActivityFactory>> activityFactoriesMock = new();
        activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new IProgramActivityFactory[]
            {
                new ProgramCreatedEventFactory(),
                new ProgramDeactivatedEventFactory(),
                new ProgramReactivatedEventFactory(),
                new ProgramUpdatedEventFactory(),
                new ProgramAssignmentUpdatedEventFactory()
            }.ToList().GetEnumerator());

        _programService = new ProgramService(
            programServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            listToFileConverters.Object,
            dateTimeFormatterMock.Object,
            activityFactoriesMock.Object);
    }

    [Fact]
    public async Task GetAllProgramsAsync_QueryIsValidOrderByNameAscending_ReturnsListOfPrograms()
    {
        // Arrange
        var programs = TestData.ProgramFaker.Generate(10);
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
            var listOfPrograms = actualResult.Query.ToList();
            listOfPrograms.Count.Should().Be(5);
            listOfPrograms.Select(x => x.Name).Should().BeInAscendingOrder();
        }
    }

    [Fact]
    public async Task GetAllProgramsAsync_QueryIsValidOrderByNameDescending_ReturnsListOfPrograms()
    {
        // Arrange
        var programs = TestData.ProgramFaker.Generate(10);
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
            var listOfPrograms = actualResult.Query.ToList();
            listOfPrograms.Count.Should().Be(5);
            listOfPrograms.Select(x => x.Name).Should().BeInDescendingOrder();
        }
    }

    [Fact]
    public async Task GetAllProgramsAsync_PageNumberIsOutOfRange_ReturnsEmptyList()
    {
        // Arrange
        var programs = TestData.ProgramFaker.Generate(10);
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
            var listOfPrograms = actualResult.Query.ToList();
            listOfPrograms.Count.Should().Be(0);
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportProgramsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var programs = TestData.ProgramFaker.Generate(5);

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

    [Fact]
    public async Task GetAllTenantProgramsAsync_QueryIsValidOrderByNameAscending_ReturnsListOfPrograms()
    {
        // Arrange
        var programs = TestData.TenantProgramFaker.Generate(10);
        await _dbContextMock.TenantsPrograms.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 5, OrderBy = "name asc", };

        // Act
        var actualResult = await _programService.GetAllTenantProgramsAsync(query);

        // Arrange
        using (new AssertionScope())
        {
            actualResult.Should().NotBeNull();
            actualResult.Count.Should().Be(10);
            var tenantProgramDtos = actualResult.Query.ToList();
            tenantProgramDtos.Count.Should().Be(5);
            tenantProgramDtos.Select(x => x.Name).Should().BeInAscendingOrder();
        }
    }

    [Fact]
    public async Task GetAllTenantProgramsAsync_QueryIsValidOrderByNameDescending_ReturnsListOfPrograms()
    {
        // Arrange
        var programs = TestData.TenantProgramFaker.Generate(10);
        await _dbContextMock.TenantsPrograms.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 5, OrderBy = "name desc", };

        // Act
        var actualResult = await _programService.GetAllTenantProgramsAsync(query);

        // Arrange
        using (new AssertionScope())
        {
            actualResult.Should().NotBeNull();
            actualResult.Count.Should().Be(10);
            var tenantProgramDtos = actualResult.Query.ToList();
            tenantProgramDtos.Count.Should().Be(5);
            tenantProgramDtos.Select(x => x.Name).Should().BeInDescendingOrder();
        }
    }

    [Fact]
    public async Task GetAllTenantProgramsAsync_PageNumberIsOutOfRange_ReturnsEmptyList()
    {
        // Arrange
        var programs = TestData.TenantProgramFaker.Generate(10);
        await _dbContextMock.TenantsPrograms.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 3, PageSize = 5, OrderBy = "name asc", };

        // Act
        var actualResult = await _programService.GetAllTenantProgramsAsync(query);

        // Arrange
        using (new AssertionScope())
        {
            actualResult.Should().NotBeNull();
            actualResult.Count.Should().Be(10);
            var tenantProgramDtos = actualResult.Query.ToList();
            tenantProgramDtos.Count.Should().Be(0);
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportTenantProgramsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var programs = TestData.TenantProgramFaker.Generate(5);

        await _dbContextMock.TenantsPrograms.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _programService.ExportTenantProgramsAsync(fileType);

        //Assert
        switch (fileType)
        {
            case FileType.Csv:
                _csvMock.Verify(x =>
                    x.Convert(It.IsAny<List<TenantProgramExportModel>>()), Times.Once());
                break;
            case FileType.Xlsx:
                _xlsxMock.Verify(x =>
                    x.Convert(It.IsAny<List<TenantProgramExportModel>>()), Times.Once());
                break;
            default:
                throw new InvalidOperationException();
        }
    }

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

    [Fact]
    public async Task GetProgramActivityHistoryAsync_ProgramExistsInTenantAndUserIsSuperAdminAndNotInTenant_ReturnsListOfProgramActivityLogsInDescendingOrderByDate()
    {
        // Arrange
        var tenant = await _dbContextMock.Tenants.FirstOrDefaultAsync();
        var program = TestData.ProgramFaker.Generate();
        program.TenantsPrograms = new List<TenantProgram> { new() { TenantId = tenant!.Id } };
        var programActivities = TestData.ProgramActivityLogFaker
            .RuleFor(x => x.ProgramId, _ => program.Id)
            .RuleFor(x => x.TenantId, _ => null)
            .Generate(2);
        var tenantProgramActivities = TestData.ProgramActivityLogFaker
            .RuleFor(x => x.ProgramId, _ => program.Id)
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .Generate(3);

        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.ProgramActivityLogs.AddRangeAsync(programActivities);
        await _dbContextMock.ProgramActivityLogs.AddRangeAsync(tenantProgramActivities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.IsSuperAdmin).Returns(true);
        _currentUserServiceMock.Setup(x => x.IsUserInTenant).Returns(false);

        // Act
        var actualResult = await _programService
            .GetProgramActivityHistoryAsync(program.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(2);
            activitiesResult.Query.Should().BeInDescendingOrder(x => x.Date);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task GetProgramActivityHistoryAsync_ProgramExistsAndUserInTenant_ReturnsListOfTenantProgramActivityLogsInDescendingOrderByDate(
        bool isSuperAdmin, bool isUserInTenant)
    {
        // Arrange
        var tenant = await _dbContextMock.Tenants.FirstOrDefaultAsync();
        var program = TestData.ProgramFaker.Generate();
        program.TenantsPrograms = new List<TenantProgram> { new() { TenantId = tenant!.Id } };
        var programActivities = TestData.ProgramActivityLogFaker
            .RuleFor(x => x.ProgramId, _ => program.Id)
            .RuleFor(x => x.TenantId, _ => null)
            .Generate(2);
        var tenantProgramActivities = TestData.ProgramActivityLogFaker
            .RuleFor(x => x.ProgramId, _ => program.Id)
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .Generate(3);

        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.ProgramActivityLogs.AddRangeAsync(programActivities);
        await _dbContextMock.ProgramActivityLogs.AddRangeAsync(tenantProgramActivities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.IsSuperAdmin).Returns(isSuperAdmin);
        _currentUserServiceMock.Setup(x => x.IsUserInTenant).Returns(isUserInTenant);

        // Act
        var actualResult = await _programService
            .GetProgramActivityHistoryAsync(program.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(3);
            activitiesResult.Query.Should().BeInDescendingOrder(x => x.Date);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task GetProgramActivityHistoryAsync_ProgramDoesNotExistInTenantAndUserInTenantId_ReturnsNotFound(
        bool isSuperAdmin, bool isUserInTenant)
    {
        // Arrange
        var program = TestData.ProgramFaker.Generate();

        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.IsSuperAdmin).Returns(isSuperAdmin);
        _currentUserServiceMock.Setup(x => x.IsUserInTenant).Returns(isUserInTenant);

        // Act
        var actualResult = await _programService.GetProgramActivityHistoryAsync(program.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

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
        }
    }

    [Fact]
    public async Task UpdateTenantProgramStatusAsync_ActiveProgramStatusAndProgramId_UpdatedProgram()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var tenantProgram = TestData.TenantProgramFaker.Generate();
        var currentDate = DateTime.UtcNow;

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TenantsPrograms.AddAsync(tenantProgram);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock
            .Setup(s => s.TenantRoles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(s => s.Roles)
            .Returns(Array.Empty<string>());

        //Act
        var actualResult = await _programService.UpdateTenantProgramStatusAsync(tenantProgram.ProgramId, Status.Active);

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var programDetails, out _).Should().BeTrue();
            programDetails.Status.Should().Be(Status.Active);
            programDetails.DeactivationDateTimeUtc.Should().BeNull();
            programDetails.ReactivationDateTimeUtc.Should().Be(currentDate);
        }
    }

    [Fact]
    public async Task UpdateTenantProgramStatusAsync_DeactivatedProgramStatusAndProgramId_UpdatedTenantProgram()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var tenantProgram = TestData.TenantProgramFaker.Generate();
        var currentDate = DateTime.UtcNow;

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TenantsPrograms.AddAsync(tenantProgram);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock
            .Setup(s => s.TenantRoles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(s => s.Roles)
            .Returns(Array.Empty<string>());

        //Act
        var actualResult =
            await _programService.UpdateTenantProgramStatusAsync(tenantProgram.ProgramId, Status.Deactivated);

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var programDetails, out _).Should().BeTrue();
            programDetails.Status.Should().Be(Status.Deactivated);
            programDetails.ReactivationDateTimeUtc.Should().BeNull();
            programDetails.DeactivationDateTimeUtc.Should().Be(currentDate);
        }
    }

    [Theory]
    [MemberData(nameof(TestData.UpdatedStatusInvalidData), MemberType = typeof(TestData))]
    public async Task UpdateTenantProgramStatusAsync_ProgramStatusAndProgramIdNotInDb_ReturnNotFound(Status status,
        Guid programId)
    {
        //Act
        var actualResult = await _programService.UpdateTenantProgramStatusAsync(programId, status);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out _, out _).Should().BeTrue();
        }
    }

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

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Faker<Tenant> TenantFaker =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static IEnumerable<object[]> UpdatedStatusInvalidData =>
            new List<object[]>
            {
                new object[] { Status.Active, Guid.NewGuid() }, new object[] { Status.Deactivated, Guid.Empty }
            };

        public static readonly Faker<Program> ProgramFaker = new Faker<Program>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Name.FirstName());

        public static readonly Faker<TenantProgram> TenantProgramFaker = new Faker<TenantProgram>()
            .RuleFor(p => p.TenantId, _ => TenantId)
            .RuleFor(p => p.Status, f => f.PickRandom<Status>())
            .RuleFor(u => u.ReactivationDateTimeUtc, _ => DateTime.UtcNow)
            .RuleFor(u => u.DeactivationDateTimeUtc, _ => DateTime.UtcNow)
            .RuleFor(p => p.Program, _ => ProgramFaker.Generate());

        public static readonly Faker<ProgramActivityLog> ProgramActivityLogFaker = new Faker<ProgramActivityLog>()
            .RuleFor(x => x.Date, f => f.Date.Recent())
            .RuleFor(x => x.Revision, _ => (uint)1)
            .RuleFor(x => x.EventType, _ => ProgramEventType.ProgramCreatedEvent)
            .RuleFor(x => x.Event, (f, x) => JsonSerializer.Serialize(
                new ProgramCreatedEvent(Guid.NewGuid(), x.Date, f.Name.FullName(), f.Name.JobTitle())
            ));

        public static readonly Faker<UpdateProgramDto> UpdateProgramDtoFaker =
            new Faker<UpdateProgramDto>()
                .CustomInstantiator(f => new UpdateProgramDto(
                    f.Lorem.Word(),
                    f.Company.CompanyName(),
                    f.Lorem.Text(),
                    f.Lorem.Word(),
                    f.Internet.Url(),
                    f.PickRandom<Jurisdiction>(),
                    f.Address.State(),
                    f.Address.Country(),
                    f.Address.City(),
                    f.Lorem.Word(),
                    f.Lorem.Word(),
                    f.Date.Past(),
                    f.Date.Past()));

        public static readonly Faker<CreateProgramDto> CreateProgramDtoFaker =
            new Faker<CreateProgramDto>()
                .CustomInstantiator(f => new CreateProgramDto(
                    f.Lorem.Word(),
                    f.Company.CompanyName(),
                    f.Lorem.Text(),
                    f.Lorem.Word(),
                    f.Internet.Url(),
                    f.PickRandom<Jurisdiction>(),
                    f.Address.State(),
                    f.Address.Country(),
                    f.Address.City(),
                    f.Lorem.Word(),
                    f.Lorem.Word(),
                    f.Date.Past(),
                    f.Date.Future()));
    }
}

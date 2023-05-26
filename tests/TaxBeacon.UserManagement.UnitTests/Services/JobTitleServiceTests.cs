using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Models.Activities.JobTitle;
using TaxBeacon.UserManagement.Models.Export;
using TaxBeacon.UserManagement.Services;
using TaxBeacon.UserManagement.Services.Activities.JobTitle;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class JobTitleServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly JobTitleService _serviceAreaService;

    public JobTitleServiceTests()
    {
        Mock<ILogger<JobTitleService>> serviceAreaServiceLoggerMock = new();
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
                .UseInMemoryDatabase($"{nameof(JobTitleServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _currentUserServiceMock = new();

        Mock<IEnumerable<IJobTitleActivityFactory>> activityFactoriesMock = new();
        activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new IJobTitleActivityFactory[]
            {
                new JobTitleUpdatedEventFactory()
            }.ToList().GetEnumerator());

        _dateTimeServiceMock = new();

        _serviceAreaService = new JobTitleService(
            serviceAreaServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            dateTimeFormatterMock.Object,
            listToFileConverters.Object,
            activityFactoriesMock.Object);
    }

    [Fact]
    public async Task GetJobTitlesAsync_TenantIdExistsAndQueryIsValidOrderByNameAscending_ReturnsJobTitles()
    {
        // Arrange
        var items = TestData.TestJobTitle.Generate(5);
        await _dbContextMock.JobTitles.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 5, OrderBy = "name asc", };

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var pageOfJobTitles = await _serviceAreaService.GetJobTitlesAsync(query, default);

        // Arrange
        using (new AssertionScope())
        {
            pageOfJobTitles.Should().NotBeNull();
            pageOfJobTitles.Count.Should().Be(5);
            var listOfJobTitles = pageOfJobTitles.Query.ToList();
            listOfJobTitles.Count.Should().Be(5);
            listOfJobTitles.Select(x => x.Name).Should().BeInAscendingOrder();
        }
    }

    [Fact]
    public async Task GetJobTitlesAsync_TenantIdExistsAndQueryIsValidOrderByNameDescending_ReturnsJobTitles()
    {
        // Arrange
        var items = TestData.TestJobTitle.Generate(5);
        await _dbContextMock.JobTitles.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 5, OrderBy = "name desc", };

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var pageOfJobTitles = await _serviceAreaService.GetJobTitlesAsync(query, default);

        // Arrange
        using (new AssertionScope())
        {
            pageOfJobTitles.Should().NotBeNull();
            pageOfJobTitles.Count.Should().Be(5);
            var listOfJobTitles = pageOfJobTitles.Query.ToList();
            listOfJobTitles.Count.Should().Be(5);
            listOfJobTitles.Select(x => x.Name).Should().BeInDescendingOrder();
        }
    }

    [Fact]
    public async Task GetJobTitlesAsync_TenantIdExistsAndPageNumberIsOutOfRange_ReturnsNotFound()
    {
        // Arrange
        var items = TestData.TestJobTitle.Generate(5);
        await _dbContextMock.JobTitles.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 2, PageSize = 5, OrderBy = "name asc", };

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var actualResult = await _serviceAreaService.GetJobTitlesAsync(query, default);

        // Arrange
        actualResult.Query.Count().Should().Be(0);
    }

    [Fact]
    public async Task GetJobTitleDetailsByIdAsync_JobTitleTenantIdExistsAndIsEqualCurrentUserTenantId_ReturnsJobTitleDto()
    {
        // Arrange
        var serviceArea = TestData.TestJobTitle.Generate();

        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.GetJobTitleDetailsByIdAsync(serviceArea.Id, default);

        // Assert
        actualResult.TryPickT0(out var serviceAreaDto, out _).Should().BeTrue();
        serviceAreaDto.Should().NotBeNull();
    }

    [Fact]
    public async Task GetJobTitleDetailsByIdAsync_NonExistingTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestJobTitle.Generate();

        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _serviceAreaService.GetJobTitleDetailsByIdAsync(Guid.NewGuid(), default);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Fact]
    public async Task GetJobTitleDetailsByIdAsync_JobTitleTenantIdNotEqualCurrentUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestJobTitle.Generate();

        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var actualResult = await _serviceAreaService.GetJobTitleDetailsByIdAsync(serviceArea.Id, default);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportJobTitlesAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var serviceAreas = TestData.TestJobTitle.Generate(5);

        await _dbContextMock.JobTitles.AddRangeAsync(serviceAreas);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        //Act
        _ = await _serviceAreaService.ExportJobTitlesAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<JobTitleExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<JobTitleExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task GetActivityHistoryAsync_JobTitleExistsAndTenantIdEqualsUserTenantId_ReturnsListOfActivityLogsInDescendingOrderByDate()
    {
        // Arrange
        var serviceArea = TestData.TestJobTitle.Generate();
        var user = TestData.TestUser.Generate();
        var activities = new[]
        {
            new JobTitleActivityLog
            {
                Date = new DateTime(2000, 01, 1),
                TenantId = serviceArea.TenantId,
                JobTitleId = serviceArea.Id,
                EventType = JobTitleEventType.JobTitleUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new JobTitleUpdatedEvent(
                        user.Id,
                        "Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "Old",
                        "New"
                    )
                )
            },
            new JobTitleActivityLog
            {
                Date = new DateTime(2000, 01, 2),
                TenantId = serviceArea.TenantId,
                JobTitleId = serviceArea.Id,
                EventType = JobTitleEventType.JobTitleUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new JobTitleUpdatedEvent(
                        user.Id,
                        "Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "Old",
                        "New"
                    )
                )
            },
            new JobTitleActivityLog
            {
                Date = new DateTime(2000, 01, 3),
                TenantId = serviceArea.TenantId,
                JobTitleId = serviceArea.Id,
                EventType = JobTitleEventType.JobTitleUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new JobTitleUpdatedEvent(
                        user.Id,
                        "Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "Old",
                        "New"
                    )
                )
            }
        };

        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.JobTitleActivityLogs.AddRangeAsync(activities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.GetActivityHistoryAsync(serviceArea.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(3);
            activitiesResult.Query.Should().BeInDescendingOrder(x => x.Date);
        }
    }

    [Fact]
    public async Task GetActivityHistory_JobTitleExistsAndTenantIdDoesNotEqualUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestJobTitle.Generate();

        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var actualResult = await _serviceAreaService.GetActivityHistoryAsync(serviceArea.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetActivityHistory_JobTitleExists_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestJobTitle.Generate();

        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.GetActivityHistoryAsync(Guid.NewGuid());

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

    [Fact]
    public async Task UpdateJobTitleDetailsAsync_JobTitleExistsAndTenantIdEqualsUserTenantId_ReturnsUpdatedJobTitleAndCapturesActivityLog()
    {
        // Arrange
        var updateJobTitleDto = TestData.UpdateJobTitleDtoFaker.Generate();
        var serviceArea = TestData.TestJobTitle.Generate();
        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.UpdateJobTitleDetailsAsync(
            serviceArea.Id, updateJobTitleDto, default);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var serviceAreaDetailsDto, out _);
            serviceAreaDetailsDto.Should().NotBeNull();
            serviceAreaDetailsDto.Id.Should().Be(serviceArea.Id);
            serviceAreaDetailsDto.Name.Should().Be(updateJobTitleDto.Name);
            serviceAreaDetailsDto.Description.Should().Be(updateJobTitleDto.Description);

            var actualActivityLog = await _dbContextMock.JobTitleActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(JobTitleEventType.JobTitleUpdatedEvent);
            actualActivityLog?.JobTitleId.Should().Be(serviceArea.Id);
            actualActivityLog?.TenantId.Should().Be(serviceArea.TenantId);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateJobTitleDetailsAsync_JobTitleExistsAndTenantIdDeosNotEqualUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var updateJobTitleDto = TestData.UpdateJobTitleDtoFaker.Generate();
        var serviceArea = TestData.TestJobTitle.Generate();
        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var actualResult = await _serviceAreaService.UpdateJobTitleDetailsAsync(
            Guid.NewGuid(), updateJobTitleDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

    [Fact]
    public async Task UpdateJobTitleDetailsAsync_JobTitleDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateJobTitleDto = TestData.UpdateJobTitleDtoFaker.Generate();
        var serviceArea = TestData.TestJobTitle.Generate();
        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.UpdateJobTitleDetailsAsync(
            Guid.NewGuid(), updateJobTitleDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetUsersAsync_JobTitleExists_ShouldReturnUsersInAscendingOrder()
    {
        // Arrange
        TestData.TestJobTitle.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(4));
        var query = new GridifyQuery { Page = 2, PageSize = 2, OrderBy = "fullname asc", };
        var serviceArea = TestData.TestJobTitle.Generate();
        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var actualResult = await _serviceAreaService.GetUsersAsync(serviceArea.Id, query, default);

        // Arrange
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var pageOfUsers, out _);
            pageOfUsers.Should().NotBeNull();
            pageOfUsers.Count.Should().Be(4);
            var listOfUsers = pageOfUsers.Query.ToList();
            listOfUsers.Count.Should().Be(2);
            listOfUsers.Select(x => x.FullName).Should().BeInAscendingOrder();
        }
    }

    [Fact]
    public async Task GetUsersAsync_JobTitleDoesNotExists_ShouldReturnNotFound()
    {
        // Arrange
        TestData.TestJobTitle.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(1));
        var query = new GridifyQuery { Page = 2, PageSize = 2, OrderBy = "fullname asc", };
        var serviceArea = TestData.TestJobTitle.Generate();
        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var actualResult = await _serviceAreaService.GetUsersAsync(new Guid(), query, default);

        // Arrange
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Fact]
    public async Task GetUsersAsync_JobTitleExists_ShouldReturnUsersInDescendingOrderByEmail()
    {
        // Arrange
        TestData.TestJobTitle.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(3));
        var query = new GridifyQuery { Page = 1, PageSize = 5, OrderBy = "email desc", };
        var serviceArea = TestData.TestJobTitle.Generate();
        await _dbContextMock.JobTitles.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var actualResult = await _serviceAreaService.GetUsersAsync(serviceArea.Id, query, default);

        // Arrange
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var pageOfUsers, out _);
            pageOfUsers.Should().NotBeNull();
            pageOfUsers.Count.Should().Be(3);
            var listOfUsers = pageOfUsers.Query.ToList();
            listOfUsers.Count.Should().Be(3);
            listOfUsers.Select(x => x.Email).Should().BeInDescendingOrder();
        }
    }

    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();

        public static readonly Faker<JobTitle> TestJobTitle =
            new Faker<JobTitle>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(t => t.TenantId, f => TestTenantId);

        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.TenantUsers, f => new List<TenantUser>()
                {
                     new TenantUser()
                     {
                         TenantId = TestTenantId,
                     }
                })
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static readonly Faker<UpdateJobTitleDto> UpdateJobTitleDtoFaker =
            new Faker<UpdateJobTitleDto>()
                .RuleFor(dto => dto.Name, f => f.Company.CompanyName())
                .RuleFor(dto => dto.Description, f => f.Lorem.Text());
    }
}

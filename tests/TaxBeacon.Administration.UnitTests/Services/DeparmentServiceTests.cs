using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.Administration.Departments;
using TaxBeacon.Administration.Departments.Activities.Factories;
using TaxBeacon.Administration.Departments.Activities.Models;
using TaxBeacon.Administration.Departments.Models;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services;

public class DepartmentServiceTests
{
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<ILogger<DepartmentService>> _departmentServiceLoggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly Mock<IDateTimeFormatter> _dateTimeFormatterMock;
    private readonly DepartmentService _departmentService;
    private readonly Mock<IEnumerable<IDepartmentActivityFactory>> _activityFactoriesMock;

    public DepartmentServiceTests()
    {
        _departmentServiceLoggerMock = new();
        _entitySaveChangesInterceptorMock = new();
        _currentUserServiceMock = new();
        _dateTimeServiceMock = new();
        _listToFileConverters = new();
        _csvMock = new();
        _xlsxMock = new();
        _dateTimeFormatterMock = new();
        _activityFactoriesMock = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        _listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _csvMock.Object, _xlsxMock.Object }.ToList()
                .GetEnumerator());

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UserServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        var currentUser = TestData.TestUser.Generate();
        _dbContextMock.Users.Add(currentUser);
        _dbContextMock.SaveChangesAsync().Wait();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        _activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new IDepartmentActivityFactory[]
            {
                new DepartmentUpdatedEventFactory()
            }.ToList().GetEnumerator());

        _departmentService = new DepartmentService(
            _departmentServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            _listToFileConverters.Object,
            _dateTimeFormatterMock.Object,
            _activityFactoriesMock.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(IDepartmentService).Assembly);
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportDepartmentsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var departments = TestData.TestDepartment.Generate(5);

        await _dbContextMock.Departments.AddRangeAsync(departments);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _departmentService.ExportDepartmentsAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<DepartmentExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<DepartmentExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportDepartmentsAsync_ValidInputDataWithDivision_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var departments = TestData.TestDepartment.Generate(5);

        await _dbContextMock.Departments.AddRangeAsync(departments);
        await _dbContextMock.SaveChangesAsync();
        _currentUserServiceMock.Setup(x => x.DivisionEnabled).Returns(true);

        //Act
        _ = await _departmentService.ExportDepartmentsAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<DepartmentWithDivisionExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<DepartmentWithDivisionExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task GetActivityHistoryAsync_TenantExists_ReturnListOfActivityLogsInDescendingOrderByDate()
    {
        var tenant = TestData.TestTenant.Generate();
        var department = TestData.TestDepartment.Generate();
        var user = TestData.TestUser.Generate();
        var activities = new[]
        {
            new DepartmentActivityLog
            {
                Date = new DateTime(2000, 1, 2),
                TenantId = tenant.Id,
                DepartmentId = department.Id,
                EventType = DepartmentEventType.DepartmentUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new DepartmentUpdatedEvent(
                        user.Id,
                        "Super Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "",
                        ""
                    )
                )
            },
            new DepartmentActivityLog
            {
                Date = new DateTime(2001, 2, 3),
                TenantId = tenant.Id,
                DepartmentId = department.Id,
                EventType = DepartmentEventType.DepartmentUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new DepartmentUpdatedEvent(
                        user.Id,
                        "Super Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "",
                        ""
                    )
                )
            },
        };

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Departments.AddAsync(department);
        await _dbContextMock.DepartmentActivityLogs.AddRangeAsync(activities);
        await _dbContextMock.SaveChangesAsync();

        var actualResult = await _departmentService.GetActivityHistoryAsync(department.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(2);
            activitiesResult.Query.Should().BeInDescendingOrder(x => x.Date);
        }
    }

    [Fact]
    public async Task GetDepartmentDetailsAsync_NonExistentDepartment_ReturnsNotFound()
    {
        // Arrange
        var department = TestData.TestDepartment.Generate();
        await _dbContextMock.Departments.AddAsync(department);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _departmentService.GetDepartmentDetailsAsync(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetDepartmentDetailsAsync_DepartmentWithNoServiceAreas_ReturnsDepartmentDetails()
    {
        // Arrange
        var department = TestData.TestDepartment.Generate();

        var jobTitles = TestData.TestJobTitle.Generate(3);
        jobTitles.ForEach(department.JobTitles.Add);

        var division = TestData.TestDivision.Generate();
        department.Division = division;

        await _dbContextMock.Departments.AddAsync(department);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _departmentService.GetDepartmentDetailsAsync(department.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var departmentDetailsResult, out _).Should().BeTrue();

            departmentDetailsResult.Id.Should().Be(department.Id);
            departmentDetailsResult.Name.Should().Be(department.Name);
            departmentDetailsResult.Description.Should().Be(department.Description);
            departmentDetailsResult.Division.Id.Should().Be(division.Id);
            departmentDetailsResult.Division.Name.Should().Be(division.Name);
            departmentDetailsResult.ServiceAreas.Should().BeEmpty();
            departmentDetailsResult.JobTitles.Count.Should().Be(jobTitles.Count);
        }
    }

    [Fact]
    public async Task GetDepartmentDetailsAsync_ExistingDepartment_ReturnsDepartmentDetails()
    {
        // Arrange
        var department = TestData.TestDepartment.Generate();

        var serviceAreas = TestData.TestServiceArea.Generate(3);
        serviceAreas.ForEach(department.ServiceAreas.Add);

        var jobTitles = TestData.TestJobTitle.Generate(3);
        jobTitles.ForEach(department.JobTitles.Add);

        await _dbContextMock.Departments.AddAsync(department);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _departmentService.GetDepartmentDetailsAsync(department.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var departmentDetailsResult, out _).Should().BeTrue();

            departmentDetailsResult.Id.Should().Be(department.Id);
            departmentDetailsResult.Name.Should().Be(department.Name);
            departmentDetailsResult.Description.Should().Be(department.Description);
            departmentDetailsResult.Division.Should().BeNull();
            departmentDetailsResult.ServiceAreas.Count().Should().Be(serviceAreas.Count);
            departmentDetailsResult.JobTitles.Count().Should().Be(jobTitles.Count);
        }
    }

    [Fact]
    public async Task UpdateDepartmentAsync_DepartmentExists_ReturnsUpdatedDepartmentAndCapturesActivityLog()
    {
        // Arrange
        var updateDepartmentDto = TestData.UpdateDepartmentDtoFaker.Generate();
        var department = TestData.TestDepartment.Generate();
        await _dbContextMock.Departments.AddAsync(department);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _departmentService.UpdateDepartmentAsync(department.Id, updateDepartmentDto, default);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var departmentDto, out _);
            departmentDto.Should().NotBeNull();
            departmentDto.Id.Should().Be(department.Id);
            departmentDto.Name.Should().Be(updateDepartmentDto.Name);

            var actualActivityLog = await _dbContextMock.DepartmentActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(DepartmentEventType.DepartmentUpdatedEvent);
            actualActivityLog?.DepartmentId.Should().Be(department.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateDepartmentAsync_DepartmentDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateDepartmentDto = TestData.UpdateDepartmentDtoFaker.Generate();
        var department = TestData.TestDepartment.Generate();

        // Act
        var actualResult = await _departmentService.UpdateDepartmentAsync(department.Id, updateDepartmentDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task QueryDepartmentUsersAsync_DepartmentExistsAndFilterApplied_ShouldReturnUsersWithSpecificDepartment()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var listOfUsers = TestData.TestUser.Generate(5);
        department.Users = listOfUsers;
        await _dbContextMock.TenantUsers.AddRangeAsync(department.Users.Select(u => new TenantUser { UserId = u.Id, TenantId = TestData.TestTenantId }));
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var query = await _departmentService.QueryDepartmentUsersAsync(department.Id);

        var teamName = listOfUsers.First().Team!.Name;
        var users = query
            .Where(u => u.Team == teamName)
            .OrderBy(u => u.Team)
            .ToArray();

        //Assert
        using (new AssertionScope())
        {
            users.Length.Should().BeGreaterThan(0);
            users.Should().BeInAscendingOrder((o1, o2) => string.Compare(o1.Team, o2.Team, StringComparison.InvariantCultureIgnoreCase));
            users.Should().AllSatisfy(u => u.Team.Should().Be(users.First().Team));
        }
    }

    [Fact]
    public async Task QueryDepartmentUsersAsync_DepartmentDoesNotExist_ShouldThrow()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var task = _departmentService.QueryDepartmentUsersAsync(new Guid());

        //Assert
        task.Exception!.InnerException.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public async Task QueryDepartmentUsersAsync_UserIsFromDifferentTenant_ShouldThrow()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        //Act
        var task = _departmentService.QueryDepartmentUsersAsync(department.Id);

        //Assert
        task.Exception!.InnerException.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public async Task GetDepartmentServiceAreasAsync_DepartmentExists_ShouldReturnDepartmentServiceAreas()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var serviceAreas = TestData.TestServiceArea.Generate(5);
        department.ServiceAreas = serviceAreas;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _departmentService.GetDepartmentServiceAreasAsync(department.Id);

        //Assert
        using (new AssertionScope())
        {
            resultOneOf.TryPickT0(out var serviceAreaDtos, out _).Should().BeTrue();
            serviceAreaDtos.Should().AllBeOfType<DepartmentServiceAreaDto>();
            serviceAreaDtos.Length.Should().Be(5);
        }
    }

    [Fact]
    public async Task GetDepartmentServiceAreasAsync_DepartmentDoesNotExist_ShouldReturnNotFound()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var serviceAreas = TestData.TestServiceArea.Generate(5);
        department.ServiceAreas = serviceAreas;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _departmentService.GetDepartmentServiceAreasAsync(Guid.NewGuid());

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task GetDepartmentServiceAreasAsync_UserIsFromDifferentTenant_ShouldReturnNotFound()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var serviceAreas = TestData.TestServiceArea.Generate(5);
        department.ServiceAreas = serviceAreas;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        //Act
        var resultOneOf = await _departmentService.GetDepartmentServiceAreasAsync(department.Id);

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task GetDepartmentJobTitlesAsync_DepartmentExists_ShouldReturnDepartmentJobTitles()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var jobTitles = TestData.TestJobTitle.Generate(5);
        department.JobTitles = jobTitles;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _departmentService.GetDepartmentJobTitlesAsync(department.Id);

        //Assert
        using (new AssertionScope())
        {
            resultOneOf.TryPickT0(out var jobTitleDtos, out _).Should().BeTrue();
            jobTitleDtos.Should().AllBeOfType<DepartmentJobTitleDto>();
            jobTitleDtos.Length.Should().Be(5);
        }
    }

    [Fact]
    public async Task GetDepartmentJobTitlesAsync_DepartmentDoesNotExist_ShouldReturnNotFound()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var jobTitles = TestData.TestJobTitle.Generate(5);
        department.JobTitles = jobTitles;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _departmentService.GetDepartmentJobTitlesAsync(Guid.NewGuid());

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task GetDepartmentJobTitlesAsync_UserIsFromDifferentTenant_ShouldReturnNotFound()
    {
        //Arrange
        var department = TestData.TestDepartment.Generate();
        department.TenantId = TestData.TestTenantId;
        var jobTitles = TestData.TestJobTitle.Generate(5);
        department.JobTitles = jobTitles;
        await _dbContextMock.Departments.AddRangeAsync(department);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        //Act
        var resultOneOf = await _departmentService.GetDepartmentJobTitlesAsync(department.Id);

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task QueryDepartments_ReturnsDepartments()
    {
        // Arrange
        var items = TestData.TestDepartment.Generate(5);
        await _dbContextMock.Departments.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var query = _departmentService.QueryDepartments();
        var result = query.ToArray();

        // Assert

        using (new AssertionScope())
        {
            result.Should().HaveCount(5);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt.ExcludingMissingMembers());
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => TestTenantId)
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<ServiceArea> TestServiceArea =
            new Faker<ServiceArea>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<Department> TestDepartment =
            new Faker<Department>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.TenantId, f => TestTenantId)
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Description, f => f.Lorem.Sentence(2))
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>())
                .RuleFor(u => u.Team, f =>
                    new Team { Name = f.Name.FirstName() })
                .RuleFor(u => u.JobTitle, f =>
                    new JobTitle { Name = f.Name.FirstName() });

        public static readonly Faker<Division> TestDivision =
            new Faker<Division>()
                .RuleFor(d => d.Id, _ => Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Name.JobType())
                .RuleFor(d => d.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(d => d.Description, f => f.Lorem.Sentence(2))
                .RuleFor(d => d.TenantId, _ => TestTenantId);

        public static readonly Faker<UpdateDepartmentDto> UpdateDepartmentDtoFaker =
            new Faker<UpdateDepartmentDto>()
                .RuleFor(dto => dto.Name, f => f.Company.CompanyName())
                .RuleFor(dto => dto.ServiceAreasIds, f => f.Make(3, Guid.NewGuid))
                .RuleFor(dto => dto.JobTitlesIds, f => f.Make(3, Guid.NewGuid))
                .RuleFor(dto => dto.DivisionId, Guid.NewGuid());

        public static readonly Faker<JobTitle> TestJobTitle = new Faker<JobTitle>()
            .RuleFor(jt => jt.Id, f => Guid.NewGuid())
            .RuleFor(jt => jt.Name, f => f.Name.JobTitle())
            .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);
    }
}

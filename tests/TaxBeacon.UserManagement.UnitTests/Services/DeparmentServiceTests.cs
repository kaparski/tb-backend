using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Mapster;
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
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services;
using TaxBeacon.UserManagement.Services.Activities.Department;

namespace TaxBeacon.UserManagement.UnitTests.Services;

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

        TypeAdapterConfig.GlobalSettings.Scan(typeof(UserMappingConfig).Assembly);
    }

    [Fact]
    public async Task GetDepartmentsAsync_AscendingOrderingAndPaginationOfLastPage_AscendingOrderOfDepartmentsAndCorrectPage()
    {
        // Arrange
        var items = TestData.TestDepartment.Generate(5);
        await _dbContextMock.Departments.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 10, OrderBy = "name asc" };

        // Act
        var itemsOneOf = await _departmentService.GetDepartmentsAsync(query, default);

        // Assert
        itemsOneOf.TryPickT0(out var pageOfDepartments, out _);
        pageOfDepartments.Should().NotBeNull();
        var listOfDepartments = pageOfDepartments.Query.ToList();
        listOfDepartments.Count.Should().Be(5);
        listOfDepartments.Select(x => x.Name).Should().BeInAscendingOrder();
        pageOfDepartments.Count.Should().Be(5);
    }

    [Fact]
    public async Task GetDepartmentsAsync_DescendingOrderingAndPaginationWithFirstPage_CorrectNumberOfDepartmentsInDescendingOrder()
    {
        // Arrange
        var items = TestData.TestDepartment.Generate(7);
        await _dbContextMock.Departments.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 4, OrderBy = "name desc" };

        // Act
        var itemsOneOf = await _departmentService.GetDepartmentsAsync(query, default);

        // Assert
        using (new AssertionScope())
        {
            itemsOneOf.TryPickT0(out var pageOfDepartments, out _);
            pageOfDepartments.Should().NotBeNull();
            var listOfDepartments = pageOfDepartments.Query.ToList();
            listOfDepartments.Count.Should().Be(4);
            listOfDepartments.Select(x => x.Name).Should().BeInDescendingOrder();
            pageOfDepartments.Count.Should().Be(7);
        }
    }

    [Fact]
    public async Task GetDepartmentsAsync_NoDepartments_CorrectNumberOfDepartments()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 123, OrderBy = "name desc" };

        // Act
        var itemsOneOf = await _departmentService.GetDepartmentsAsync(query, default);

        // Assert
        using (new AssertionScope())
        {
            itemsOneOf.TryPickT0(out var pageOfDepartments, out _);
            pageOfDepartments.Should().NotBeNull();
            var listOfDepartments = pageOfDepartments.Query.ToList();
            listOfDepartments.Count.Should().Be(0);
            pageOfDepartments.Count.Should().Be(0);
        }
    }

    [Fact]
    public async Task GetDepartmentsAsync_PageNumberOutsideOfTotalRange_DepartmentListIsEmpty()
    {
        // Arrange
        var items = TestData.TestDepartment.Generate(7);
        await _dbContextMock.Departments.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 2, PageSize = 25, OrderBy = "name asc", };

        // Act
        var itemsOneOf = await _departmentService.GetDepartmentsAsync(query, default);

        // Assert
        itemsOneOf.TryPickT0(out var pageOfDepartments, out _);
        pageOfDepartments.Should().BeNull();
    }

    [Fact]
    public async Task GetDepartmentsAsync_PageNumberRightOutsideOfTotalRange_DepartmentListIsEmpty()
    {
        // Arrange
        var items = TestData.TestDepartment.Generate(10);
        await _dbContextMock.Departments.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 3, PageSize = 5, OrderBy = "name asc", };

        // Act
        var itemsOneOf = await _departmentService.GetDepartmentsAsync(query, default);

        // Assert
        itemsOneOf.TryPickT0(out var pageOfDepartments, out _);
        pageOfDepartments.Should().BeNull();
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

        var serviceAreas = TestData.TestServiceArea.Generate(3);
        serviceAreas.ForEach(department.ServiceAreas.Add);

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
            departmentDetailsResult.DivisionId.Should().Be(division.Id);
            departmentDetailsResult.Division.Should().Be(division.Name);
            departmentDetailsResult.ServiceAreas.Count.Should().Be(serviceAreas.Count);
        }
    }

    [Fact]
    public async Task GetDepartmentDetailsAsync_ExistingDepartment_ReturnsDepartmentDetails()
    {
        // Arrange
        var department = TestData.TestDepartment.Generate();
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
            departmentDetailsResult.Division.Should().BeEmpty();
            departmentDetailsResult.ServiceAreas.Should().BeEmpty();
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
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static readonly Faker<Division> TestDivision =
            new Faker<Division>()
                .RuleFor(d => d.Id, _ => Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Name.JobType())
                .RuleFor(d => d.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(d => d.Description, f => f.Lorem.Sentence(2))
                .RuleFor(d => d.TenantId, _ => TestTenantId);

        public static readonly Faker<UpdateDepartmentDto> UpdateDepartmentDtoFaker =
            new Faker<UpdateDepartmentDto>()
                .RuleFor(dto => dto.Name, f => f.Company.CompanyName());
    }
}

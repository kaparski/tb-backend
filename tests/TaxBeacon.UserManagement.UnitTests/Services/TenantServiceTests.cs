using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf.Types;
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
using TaxBeacon.UserManagement.Models.Activities.Tenant;
using TaxBeacon.UserManagement.Services;
using TaxBeacon.UserManagement.Services.Activities.Tenant;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class TenantServiceTests
{
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<ILogger<TenantService>> _tenantServiceLoggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly Mock<IDateTimeFormatter> _dateTimeFormatterMock;
    private readonly TenantService _tenantService;
    private readonly Mock<IEnumerable<ITenantActivityFactory>> _activityFactoriesMock;
    public static readonly Guid TenantId = Guid.NewGuid();

    public TenantServiceTests()
    {
        _tenantServiceLoggerMock = new();
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

        _currentUserServiceMock = new();
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TenantId);

        var currentUser = TestData.TestUser.Generate();
        _dbContextMock.Users.Add(currentUser);
        _dbContextMock.SaveChangesAsync().Wait();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        _activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new ITenantActivityFactory[]
            {
                new TenantEnteredEventFactory(), new TenantExitedEventFactory(), new TenantUpdatedEventFactory()
            }.ToList().GetEnumerator());

        _tenantService = new TenantService(
            _tenantServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            _listToFileConverters.Object,
            _dateTimeFormatterMock.Object,
            _activityFactoriesMock.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(ITenantService).Assembly);
    }

    [Fact]
    public async Task GetTenantsAsync_AscendingOrderingAndPaginationOfLastPage_AscendingOrderOfTenantsAndCorrectPage()
    {
        // Arrange
        var tenants = TestData.TestTenant.Generate(5);
        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 10, OrderBy = "name asc" };

        // Act
        var pageOfTenants = await _tenantService.GetTenantsAsync(query);

        // Assert
        pageOfTenants.Should().NotBeNull();
        var listOfTenants = pageOfTenants.Query.ToList();
        listOfTenants.Count.Should().Be(5);
        listOfTenants.Select(x => x.Name).Should().BeInAscendingOrder();
        pageOfTenants.Count.Should().Be(5);
    }

    [Fact]
    public async Task GetTenantsAsync_DescendingOrderingAndPaginationWithFirstPage_CorrectNumberOfTenantsInDescendingOrder()
    {
        // Arrange
        var tenants = TestData.TestTenant.Generate(7);
        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 4, OrderBy = "name desc" };

        // Act
        var pageOfTenants = await _tenantService.GetTenantsAsync(query);

        // Assert
        using (new AssertionScope())
        {
            pageOfTenants.Should().NotBeNull();
            var listOfTenants = pageOfTenants.Query.ToList();
            listOfTenants.Count.Should().Be(4);
            listOfTenants.Select(x => x.Name).Should().BeInDescendingOrder();
            pageOfTenants.Count.Should().Be(7);
        }
    }

    [Fact]
    public async Task GetTenantsAsync_NoTenants_CorrectNumberOfTenants()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 123, OrderBy = "name desc" };

        // Act
        var pageOfTenants = await _tenantService.GetTenantsAsync(query);

        // Assert
        using (new AssertionScope())
        {
            pageOfTenants.Should().NotBeNull();
            var listOfTenants = pageOfTenants.Query.ToList();
            listOfTenants.Count.Should().Be(0);
            pageOfTenants.Count.Should().Be(0);
        }
    }

    [Fact]
    public async Task GetTenantsAsync_PageNumberOutsideOfTotalRange_TenantListIsEmpty()
    {
        // Arrange
        var tenants = TestData.TestTenant.Generate(7);
        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 2, PageSize = 25, OrderBy = "name asc" };

        // Act
        var pageOfTenants = await _tenantService.GetTenantsAsync(query);

        // Assert
        pageOfTenants.Query.Count().Should().Be(0);
    }

    [Fact]
    public async Task GetTenantsAsync_PageNumberRightOutsideOfTotalRange_TenantListIsEmpty()
    {
        // Arrange
        var tenants = TestData.TestTenant.Generate(10);
        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 3, PageSize = 5, OrderBy = "name asc" };

        // Act
        var pageOfTenants = await _tenantService.GetTenantsAsync(query);

        // Assert
        pageOfTenants.Query.Count().Should().Be(0);
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportTenantsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var tenants = TestData.TestTenant.Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _tenantService.ExportTenantsAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<TenantExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<TenantExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task GetTenantByIdAsync_ExistingTenantId_ReturnsTenantDto()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _tenantService.GetTenantByIdAsync(tenant.Id);

        // Assert
        actualResult.TryPickT0(out var tenantDto, out _);
        tenantDto.Should().NotBeNull();
    }

    [Fact]
    public async Task GetTenantByIdAsync_NonExistingTenantId_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TestTenant.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _tenantService.GetTenantByIdAsync(Guid.NewGuid());

        // Assert
        actualResult.TryPickT1(out var _, out var _).Should().BeTrue();
    }

    [Fact]
    public async Task GetActivityHistoryAsync_TenantExists_ReturnListOfActivityLogsInDescendingOrderByDate()
    {
        var tenant = TestData.TestTenant.Generate();
        var user = TestData.TestUser.Generate();
        var activities = new[]
        {
            new TenantActivityLog
            {
                Date = new DateTime(2000, 01, 1),
                TenantId = tenant.Id,
                EventType = TenantEventType.TenantEnteredEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new TenantEnteredEvent(
                        user.Id,
                        "Super Admin",
                        user.FullName,
                        DateTime.UtcNow
                    )
                )
            },
            new TenantActivityLog
            {
                Date = new DateTime(2000, 01, 2),
                TenantId = tenant.Id,
                EventType = TenantEventType.TenantUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new TenantUpdatedEvent(
                        user.Id,
                        "Super Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "",
                        ""
                    )
                )
            },
            new TenantActivityLog
            {
                Date = new DateTime(2000, 01, 3),
                TenantId = tenant.Id,
                EventType = TenantEventType.TenantExitedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new TenantExitedEvent(
                        user.Id,
                        "Super Admin",
                        user.FullName,
                        DateTime.UtcNow
                    )
                )
            }
        };

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TenantActivityLogs.AddRangeAsync(activities);
        await _dbContextMock.SaveChangesAsync();

        var actualResult = await _tenantService.GetActivityHistoryAsync(tenant.Id);

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
    public async Task GetActivityHistoryAsync_TenantDoesNotExist_ReturnsNotFound()
    {
        var tenant = TestData.TestTenant.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var actualResult = await _tenantService.GetActivityHistoryAsync(Guid.NewGuid());

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var _, out var notFound);
            notFound.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task UpdateTenantAsync_TenantExists_ReturnsUpdatedTenantAndCapturesActivityLog()
    {
        // Arrange
        var updateTenantDto = TestData.UpdateTenantDtoFaker.Generate();
        var tenant = TestData.TestTenant.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _tenantService.UpdateTenantAsync(tenant.Id, updateTenantDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var tenantDto, out _);
            tenantDto.Should().NotBeNull();
            tenantDto.Id.Should().Be(tenant.Id);
            tenantDto.Name.Should().Be(updateTenantDto.Name);

            var actualActivityLog = await _dbContextMock.TenantActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(TenantEventType.TenantUpdatedEvent);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateTenantAsync_TenantDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateTenantDto = TestData.UpdateTenantDtoFaker.Generate();
        var tenant = TestData.TestTenant.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _tenantService.UpdateTenantAsync(Guid.NewGuid(), updateTenantDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var tenantDto, out _);
            tenantDto.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task SwitchToTenantAsync_UserIsNotSuperAdmin_Throws()
    {
        //Act
        var act = async () => await _tenantService.SwitchToTenantAsync(null, TestData.TestTenantId);

        //Assert
        using (new AssertionScope())
        {
            await act.Should()
                .ThrowAsync<InvalidOperationException>()
                .WithMessage("Sequence contains no elements");
        }
    }

    [Fact]
    public async Task SwitchToTenantAsync_NoOldTenant_ProducesTenantEnteredEvent()
    {
        //Arrange
        var currentUserId = _currentUserServiceMock.Object.UserId;
        var currentUser = await _dbContextMock.Users.SingleAsync(u => u.Id == currentUserId);
        currentUser.UserRoles.Add(new UserRole { Role = new Role { Name = "Super admin" } });
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _tenantService.SwitchToTenantAsync(null, TestData.TestTenantId);

        //Assert
        using (new AssertionScope())
        {
            var item = _dbContextMock.TenantActivityLogs.Single();
            var evt = JsonSerializer.Deserialize<TenantEnteredEvent>(item.Event);

            item.TenantId.Should().Be(TestData.TestTenantId);
            item.EventType.Should().Be(TenantEventType.TenantEnteredEvent);
            evt?.ExecutorId.Should().Be(currentUserId);
            evt?.ExecutorFullName.Should().Be(currentUser.FullName);
        }
    }

    [Fact]
    public async Task SwitchToTenantAsync_NoNewTenant_ProducesTenantEnteredEvent()
    {
        //Arrange
        var currentUserId = _currentUserServiceMock.Object.UserId;
        var currentUser = await _dbContextMock.Users.SingleAsync(u => u.Id == currentUserId);
        currentUser.UserRoles.Add(new UserRole { Role = new Role { Name = "Super admin" } });
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _tenantService.SwitchToTenantAsync(TestData.TestTenantId, null);

        //Assert
        using (new AssertionScope())
        {
            var item = _dbContextMock.TenantActivityLogs.Single();
            var evt = JsonSerializer.Deserialize<TenantExitedEvent>(item.Event);

            item.TenantId.Should().Be(TestData.TestTenantId);
            item.EventType.Should().Be(TenantEventType.TenantExitedEvent);
            evt?.ExecutorId.Should().Be(currentUserId);
            evt?.ExecutorFullName.Should().Be(currentUser.FullName);
        }
    }

    [Fact]
    public async Task SwitchToTenantAsync_ProducesBothEvents()
    {
        //Arrange
        var currentUserId = _currentUserServiceMock.Object.UserId;
        var currentUser = await _dbContextMock.Users.SingleAsync(u => u.Id == currentUserId);
        currentUser.UserRoles.Add(new UserRole { Role = new Role { Name = "Super admin" } });
        await _dbContextMock.SaveChangesAsync();

        var oldTenantId = Guid.NewGuid();
        var newTenantId = Guid.NewGuid();

        //Act
        await _tenantService.SwitchToTenantAsync(oldTenantId, newTenantId);

        //Assert
        using (new AssertionScope())
        {
            var items = _dbContextMock.TenantActivityLogs.OrderBy(a => a.Date).ToList();
            items.Should().HaveCount(2);

            var exitEvt = JsonSerializer.Deserialize<TenantEnteredEvent>(items[0].Event);
            var enterEvt = JsonSerializer.Deserialize<TenantEnteredEvent>(items[1].Event);

            items[0].TenantId.Should().Be(oldTenantId);
            items[0].EventType.Should().Be(TenantEventType.TenantExitedEvent);
            exitEvt?.ExecutorId.Should().Be(currentUserId);
            exitEvt?.ExecutorFullName.Should().Be(currentUser.FullName);

            items[1].TenantId.Should().Be(newTenantId);
            items[1].EventType.Should().Be(TenantEventType.TenantEnteredEvent);
            enterEvt?.ExecutorId.Should().Be(currentUserId);
            enterEvt?.ExecutorFullName.Should().Be(currentUser.FullName);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ToggleDivisionsAsync_DivisionsState_ShouldToggleDivision(bool divisionEnabled)
    {
        //Arrange
        var currentTenantId = _currentUserServiceMock.Object.TenantId;
        var currentTenant = new Tenant()
        {
            Id = TenantId,
            Name = "TestTenant",
            DivisionEnabled = divisionEnabled,
        };
        _dbContextMock.Tenants.Add(currentTenant);
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _tenantService.ToggleDivisionsAsync(!divisionEnabled, default);

        //Assert
        using (new AssertionScope())
        {
            currentTenant = await _dbContextMock.Tenants.Where(x => x.Id == currentTenantId).FirstAsync();
            currentTenant.DivisionEnabled.Should().Be(!divisionEnabled);
        }
    }

    [Fact]
    public async Task ToggleDivisionsAsync_TenantDoesNotExist_ShouldReturnNotFound()
    {
        //Arrange
        _dbContextMock.Tenants.Add(new Tenant()
        {
            Id = TenantId,
            Name = "TestTenant",
        });
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        //Act
        var oneOfNotFound = await _tenantService.ToggleDivisionsAsync(false, default);

        //Assert
        oneOfNotFound.IsT1.Should().BeTrue();
    }

    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();

        public static readonly Faker<Department> TestDepartment =
            new Faker<Department>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.TenantId, f => TestTenantId)
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static readonly Faker<UpdateTenantDto> UpdateTenantDtoFaker =
            new Faker<UpdateTenantDto>()
                .RuleFor(dto => dto.Name, f => f.Company.CompanyName());
    }
}

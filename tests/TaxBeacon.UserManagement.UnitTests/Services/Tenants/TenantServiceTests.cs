using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Mapster;
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
using TaxBeacon.UserManagement.Tenants;
using TaxBeacon.UserManagement.Tenants.Activities;
using TaxBeacon.UserManagement.Tenants.Activities.Models;
using TaxBeacon.UserManagement.Tenants.Models;

namespace TaxBeacon.UserManagement.UnitTests.Services.Tenants;

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
        var tenants = TestData.TenantFaker.Generate(5);
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
    public async Task
        GetTenantsAsync_DescendingOrderingAndPaginationWithFirstPage_CorrectNumberOfTenantsInDescendingOrder()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(7);
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
        var tenants = TestData.TenantFaker.Generate(7);
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
        var tenants = TestData.TenantFaker.Generate(10);
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
        var tenants = TestData.TenantFaker.Generate(5);

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
        var tenant = TestData.TenantFaker.Generate();

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
        var tenant = TestData.TenantFaker.Generate();

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
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
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
        var tenant = TestData.TenantFaker.Generate();

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
        var tenant = TestData.TenantFaker.Generate();
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
        var tenant = TestData.TenantFaker.Generate();
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
        var user = TestData.UserFaker.Generate();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = Common.Roles.Roles.SuperAdmin } });
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        //Act
        await _tenantService.SwitchToTenantAsync(null, TestData.TestTenantId);

        //Assert
        using (new AssertionScope())
        {
            var item = _dbContextMock.TenantActivityLogs.Single();
            var evt = JsonSerializer.Deserialize<TenantEnteredEvent>(item.Event);

            item.TenantId.Should().Be(TestData.TestTenantId);
            item.EventType.Should().Be(TenantEventType.TenantEnteredEvent);
            evt?.ExecutorId.Should().Be(user.Id);
            evt?.ExecutorFullName.Should().Be(user.FullName);
        }
    }

    [Fact]
    public async Task SwitchToTenantAsync_NoNewTenant_ProducesTenantEnteredEvent()
    {
        //Arrange
        var user = TestData.UserFaker.Generate();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = Common.Roles.Roles.SuperAdmin } });
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        //Act
        await _tenantService.SwitchToTenantAsync(TestData.TestTenantId, null);

        //Assert
        using (new AssertionScope())
        {
            var item = _dbContextMock.TenantActivityLogs.Single();
            var evt = JsonSerializer.Deserialize<TenantExitedEvent>(item.Event);

            item.TenantId.Should().Be(TestData.TestTenantId);
            item.EventType.Should().Be(TenantEventType.TenantExitedEvent);
            evt?.ExecutorId.Should().Be(user.Id);
            evt?.ExecutorFullName.Should().Be(user.FullName);
        }
    }

    [Fact]
    public async Task SwitchToTenantAsync_ProducesBothEvents()
    {
        //Arrange
        var user = TestData.UserFaker.Generate();
        user.UserRoles.Add(new UserRole { Role = new Role { Name = Common.Roles.Roles.SuperAdmin } });
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

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
            exitEvt?.ExecutorId.Should().Be(user.Id);
            exitEvt?.ExecutorFullName.Should().Be(user.FullName);

            items[1].TenantId.Should().Be(newTenantId);
            items[1].EventType.Should().Be(TenantEventType.TenantEnteredEvent);
            enterEvt?.ExecutorId.Should().Be(user.Id);
            enterEvt?.ExecutorFullName.Should().Be(user.FullName);
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task ToggleDivisionsAsync_DivisionsState_ShouldToggleDivision(bool divisionEnabled)
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        tenant.DivisionEnabled = divisionEnabled;

        _dbContextMock.Tenants.Add(tenant);
        await _dbContextMock.SaveChangesAsync();

        // Act
        await _tenantService.ToggleDivisionsAsync(divisionEnabled);

        // Assert
        using (new AssertionScope())
        {
            var actualTenant = await _dbContextMock.Tenants.FirstAsync(x => x.Id == tenant.Id);
            actualTenant.DivisionEnabled.Should().Be(divisionEnabled);
        }
    }

    [Fact]
    public async Task ToggleDivisionsAsync_TenantDoesNotExist_ShouldReturnNotFound()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var oneOfNotFound = await _tenantService.ToggleDivisionsAsync(false);

        // Assert
        oneOfNotFound.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task GetAssignedTenantProgramsAsync_ExistingTenant_ReturnsListOfPrograms()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        tenants[0].TenantsPrograms.First().IsDeleted = true;
        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _tenantService.GetTenantProgramsAsync(tenants[0].Id);

        // Assert
        actualResult.TryPickT0(out var actualPrograms, out _).Should().BeTrue();
        var actualProgramsIds = actualPrograms.Select(ap => ap.Id);
        actualProgramsIds.Should()
            .BeEquivalentTo(tenants[0].TenantsPrograms
                .Where(tp => tp.IsDeleted == false)
                .Select(tp => tp.ProgramId));
    }

    [Fact]
    public async Task GetAssignedTenantProgramsAsync_NotExistingTenantId_ReturnsNotFound()
    {
        // Act
        var actualResult = await _tenantService.GetTenantProgramsAsync(Guid.NewGuid());

        // Assert
        actualResult.IsT0.Should().BeFalse();
        actualResult.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task ChangeTenantProgramsAsync_TenantWithoutProgramsAndNewProgramsIds_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker
            .RuleFor(t => t.TenantsPrograms, _ => Enumerable.Empty<TenantProgram>().ToList())
            .Generate();
        var programs = TestData.ProgramFaker.Generate(3);
        var programsIds = programs.Select(p => p.Id).ToList();

        await _dbContextMock.Tenants.AddRangeAsync(tenant);
        await _dbContextMock.Programs.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .Setup(s => s.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _tenantService.ChangeTenantProgramsAsync(tenant.Id, programsIds);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out _, out _).Should().BeTrue();

            var actualTenantProgramsId = await _dbContextMock.TenantsPrograms
                .Where(tp => tp.TenantId == tenant.Id && tp.IsDeleted == false)
                .Select(tp => tp.ProgramId)
                .ToListAsync();
            actualTenantProgramsId.Count.Should().Be(3);
            actualTenantProgramsId.Should().Contain(programsIds);

            var actualActivityLog = await _dbContextMock.TenantActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(TenantEventType.TenantAssignProgramsEvent);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(2));
        }
    }

    [Fact]
    public async Task ChangeTenantProgramsAsync_TenantWithDisabledProgramsAndReactivateProgramsIds_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        foreach (var tenantTenantsProgram in tenant.TenantsPrograms)
        {
            tenantTenantsProgram.IsDeleted = true;
            tenantTenantsProgram.DeletedDateTimeUtc = DateTime.UtcNow;
        }

        await _dbContextMock.Tenants.AddRangeAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var reactivateProgramIds = new[] { tenant.TenantsPrograms.First().ProgramId };
        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .Setup(s => s.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _tenantService.ChangeTenantProgramsAsync(tenant.Id, reactivateProgramIds);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out _, out _).Should().BeTrue();

            var actualTenantProgramsId = await _dbContextMock.TenantsPrograms
                .Where(tp => tp.TenantId == tenant.Id && tp.IsDeleted == false)
                .Select(tp => tp.ProgramId)
                .ToListAsync();
            actualTenantProgramsId.Count.Should().Be(1);
            actualTenantProgramsId.Should().BeEquivalentTo(reactivateProgramIds);

            var actualActivityLog = await _dbContextMock.TenantActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(TenantEventType.TenantAssignProgramsEvent);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(2));
        }
    }

    [Fact]
    public async Task ChangeTenantProgramsAsync_TenantWithProgramsAndNewProgramsIds_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();

        await _dbContextMock.Tenants.AddRangeAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var programsIds = new[] { tenant.TenantsPrograms.First().ProgramId };
        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .SetupSequence(s => s.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _tenantService.ChangeTenantProgramsAsync(tenant.Id, programsIds);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out _, out _).Should().BeTrue();

            var actualTenantProgramsId = await _dbContextMock.TenantsPrograms
                .Where(tp => tp.TenantId == tenant.Id && tp.IsDeleted == false)
                .Select(tp => tp.ProgramId)
                .ToListAsync();
            actualTenantProgramsId.Count.Should().Be(1);
            actualTenantProgramsId.Should().Contain(programsIds);

            var actualActivityLog = await _dbContextMock.TenantActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(TenantEventType.TenantUnassignProgramEvent);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(2));
        }
    }

    [Fact]
    public async Task ChangeTenantProgramsAsync_NotExistingTenantId_ReturnsNotFound()
    {
        // Act
        var actualResult = await _tenantService.ChangeTenantProgramsAsync(Guid.NewGuid(), Array.Empty<Guid>());

        // Assert
        actualResult.IsT0.Should().BeFalse();
        actualResult.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task QueryTenants_ReturnsTenants()
    {
        // Arrange
        var items = TestData.TenantFaker.Generate(5);
        await _dbContextMock.Tenants.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var query = _tenantService.QueryTenants();
        var result = query.ToArray();

        // Assert

        using (new AssertionScope())
        {
            result.Should().HaveCount(5);

            foreach (var dto in result)
            {
                var item = items.Single(u => u.Id == dto.Id);

                dto.Should().BeEquivalentTo(item, opt => opt
                    .ExcludingMissingMembers()
                    .Excluding(d => d.Departments)
                );
            }
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Guid TestTenantId = Guid.NewGuid();

        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(t => t.TenantsPrograms, _ => ProgramFaker.Generate(3)
                    .Select(p => new TenantProgram { Program = p, IsDeleted = false }).ToList());

        public static Faker<User> UserFaker =>
            new Faker<User>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static Faker<UpdateTenantDto> UpdateTenantDtoFaker =>
            new Faker<UpdateTenantDto>()
                .CustomInstantiator(f => new UpdateTenantDto(f.Company.CompanyName()));

        public static Faker<Program> ProgramFaker => new Faker<Program>()
            .RuleFor(p => p.Id, _ => Guid.NewGuid())
            .RuleFor(p => p.Name, f => f.Name.FirstName());
    }
}

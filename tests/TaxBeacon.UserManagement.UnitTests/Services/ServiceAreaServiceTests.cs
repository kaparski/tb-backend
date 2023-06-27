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
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.ServiceAreas;
using TaxBeacon.UserManagement.ServiceAreas.Activities.Factories;
using TaxBeacon.UserManagement.ServiceAreas.Activities.Models;
using TaxBeacon.UserManagement.ServiceAreas.Models;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class ServiceAreaServiceTests
{
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly ServiceAreaService _serviceAreaService;

    public ServiceAreaServiceTests()
    {
        Mock<ILogger<ServiceAreaService>> serviceAreaServiceLoggerMock = new();
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
                .UseInMemoryDatabase($"{nameof(ServiceAreaServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            entitySaveChangesInterceptorMock.Object);

        _currentUserServiceMock = new();

        Mock<IEnumerable<IServiceAreaActivityFactory>> activityFactoriesMock = new();
        activityFactoriesMock
            .Setup(x => x.GetEnumerator())
            .Returns(new IServiceAreaActivityFactory[]
            {
                new ServiceAreaUpdatedEventFactory()
            }.ToList().GetEnumerator());

        _dateTimeServiceMock = new();

        _serviceAreaService = new ServiceAreaService(
            serviceAreaServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            dateTimeFormatterMock.Object,
            listToFileConverters.Object,
            activityFactoriesMock.Object);
    }

    [Fact]
    public async Task GetServiceAreasAsync_TenantIdExistsAndQueryIsValidOrderByNameAscending_ReturnsServiceAreas()
    {
        // Arrange
        var items = TestData.TestServiceArea.Generate(5);
        await _dbContextMock.ServiceAreas.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 5, OrderBy = "name asc", };

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var pageOfServiceAreas = await _serviceAreaService.GetServiceAreasAsync(query, default);

        // Arrange
        using (new AssertionScope())
        {
            pageOfServiceAreas.Should().NotBeNull();
            pageOfServiceAreas.Count.Should().Be(5);
            var listOfServiceAreas = pageOfServiceAreas.Query.ToList();
            listOfServiceAreas.Count.Should().Be(5);
            listOfServiceAreas.Select(x => x.Name).Should().BeInAscendingOrder();
        }
    }

    [Fact]
    public async Task GetServiceAreasAsync_TenantIdExistsAndQueryIsValidOrderByNameDescending_ReturnsServiceAreas()
    {
        // Arrange
        var items = TestData.TestServiceArea.Generate(5);
        await _dbContextMock.ServiceAreas.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 1, PageSize = 5, OrderBy = "name desc", };

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var pageOfServiceAreas = await _serviceAreaService.GetServiceAreasAsync(query, default);

        // Arrange
        using (new AssertionScope())
        {
            pageOfServiceAreas.Should().NotBeNull();
            pageOfServiceAreas.Count.Should().Be(5);
            var listOfServiceAreas = pageOfServiceAreas.Query.ToList();
            listOfServiceAreas.Count.Should().Be(5);
            listOfServiceAreas.Select(x => x.Name).Should().BeInDescendingOrder();
        }
    }

    [Fact]
    public async Task GetServiceAreasAsync_TenantIdExistsAndPageNumberIsOutOfRange_ReturnsNotFound()
    {
        // Arrange
        var items = TestData.TestServiceArea.Generate(5);
        await _dbContextMock.ServiceAreas.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();
        var query = new GridifyQuery { Page = 2, PageSize = 5, OrderBy = "name asc", };

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var actualResult = await _serviceAreaService.GetServiceAreasAsync(query, default);

        // Arrange
        actualResult.Query.Count().Should().Be(0);
    }

    [Fact]
    public async Task GetServiceAreaDetailsByIdAsync_ServiceAreaTenantIdExistsAndIsEqualCurrentUserTenantId_ReturnsServiceAreaDto()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.GetServiceAreaDetailsByIdAsync(serviceArea.Id, default);

        // Assert
        actualResult.TryPickT0(out var serviceAreaDto, out _).Should().BeTrue();
        serviceAreaDto.Should().NotBeNull();
    }

    [Fact]
    public async Task GetServiceAreaDetailsByIdAsync_NonExistingTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var actualResult = await _serviceAreaService.GetServiceAreaDetailsByIdAsync(Guid.NewGuid(), default);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Fact]
    public async Task GetServiceAreaDetailsByIdAsync_ServiceAreaTenantIdNotEqualCurrentUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var actualResult = await _serviceAreaService.GetServiceAreaDetailsByIdAsync(serviceArea.Id, default);

        // Assert
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportServiceAreasAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var serviceAreas = TestData.TestServiceArea.Generate(5);

        await _dbContextMock.ServiceAreas.AddRangeAsync(serviceAreas);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        //Act
        _ = await _serviceAreaService.ExportServiceAreasAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<ServiceAreaExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<ServiceAreaExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task GetActivityHistoryAsync_ServiceAreaExistsAndTenantIdEqualsUserTenantId_ReturnsListOfActivityLogsInDescendingOrderByDate()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();
        var user = TestData.TestUser.Generate();
        var activities = new[]
        {
            new ServiceAreaActivityLog
            {
                Date = new DateTime(2000, 01, 1),
                TenantId = serviceArea.TenantId,
                ServiceAreaId = serviceArea.Id,
                EventType = ServiceAreaEventType.ServiceAreaUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new ServiceAreaUpdatedEvent(
                        user.Id,
                        "Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "Old",
                        "New"
                    )
                )
            },
            new ServiceAreaActivityLog
            {
                Date = new DateTime(2000, 01, 2),
                TenantId = serviceArea.TenantId,
                ServiceAreaId = serviceArea.Id,
                EventType = ServiceAreaEventType.ServiceAreaUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new ServiceAreaUpdatedEvent(
                        user.Id,
                        "Admin",
                        user.FullName,
                        DateTime.UtcNow,
                        "Old",
                        "New"
                    )
                )
            },
            new ServiceAreaActivityLog
            {
                Date = new DateTime(2000, 01, 3),
                TenantId = serviceArea.TenantId,
                ServiceAreaId = serviceArea.Id,
                EventType = ServiceAreaEventType.ServiceAreaUpdatedEvent,
                Revision = 1,
                Event = JsonSerializer.Serialize(new ServiceAreaUpdatedEvent(
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

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.ServiceAreaActivityLogs.AddRangeAsync(activities);
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
    public async Task GetActivityHistory_ServiceAreaExistsAndTenantIdDoesNotEqualUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
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
    public async Task GetActivityHistory_ServiceAreaExists_ReturnsNotFound()
    {
        // Arrange
        var serviceArea = TestData.TestServiceArea.Generate();

        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
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
    public async Task UpdateServiceAreaDetailsAsync_ServiceAreaExistsAndTenantIdEqualsUserTenantId_ReturnsUpdatedServiceAreaAndCapturesActivityLog()
    {
        // Arrange
        var updateServiceAreaDto = TestData.UpdateServiceAreaDtoFaker.Generate();
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.UpdateServiceAreaDetailsAsync(
            serviceArea.Id, updateServiceAreaDto, default);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var serviceAreaDetailsDto, out _);
            serviceAreaDetailsDto.Should().NotBeNull();
            serviceAreaDetailsDto.Id.Should().Be(serviceArea.Id);
            serviceAreaDetailsDto.Name.Should().Be(updateServiceAreaDto.Name);
            serviceAreaDetailsDto.Description.Should().Be(updateServiceAreaDto.Description);

            var actualActivityLog = await _dbContextMock.ServiceAreaActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(ServiceAreaEventType.ServiceAreaUpdatedEvent);
            actualActivityLog?.ServiceAreaId.Should().Be(serviceArea.Id);
            actualActivityLog?.TenantId.Should().Be(serviceArea.TenantId);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateServiceAreaDetailsAsync_ServiceAreaExistsAndTenantIdDeosNotEqualUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var updateServiceAreaDto = TestData.UpdateServiceAreaDtoFaker.Generate();
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        // Act
        var actualResult = await _serviceAreaService.UpdateServiceAreaDetailsAsync(
            Guid.NewGuid(), updateServiceAreaDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

    [Fact]
    public async Task UpdateServiceAreaDetailsAsync_ServiceAreaDoesNotExist_ReturnsNotFound()
    {
        // Arrange
        var updateServiceAreaDto = TestData.UpdateServiceAreaDtoFaker.Generate();
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(serviceArea.TenantId);

        // Act
        var actualResult = await _serviceAreaService.UpdateServiceAreaDetailsAsync(
            Guid.NewGuid(), updateServiceAreaDto, default);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetUsersAsync_ServiceAreaExists_ShouldReturnUsersInAscendingOrder()
    {
        // Arrange
        TestData.TestServiceArea.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(4));
        var query = new GridifyQuery { Page = 2, PageSize = 2, OrderBy = "fullname asc", };
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
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
    public async Task GetUsersAsync_ServiceAreaDoesNotExists_ShouldReturnNotFound()
    {
        // Arrange
        TestData.TestServiceArea.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(1));
        var query = new GridifyQuery { Page = 2, PageSize = 2, OrderBy = "fullname asc", };
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var actualResult = await _serviceAreaService.GetUsersAsync(new Guid(), query, default);

        // Arrange
        actualResult.IsT1.Should().BeTrue();
        actualResult.IsT0.Should().BeFalse();
    }

    [Fact]
    public async Task GetUsersAsync_ServiceAreaExists_ShouldReturnUsersInDescendingOrderByEmail()
    {
        // Arrange
        TestData.TestServiceArea.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(3));
        var query = new GridifyQuery { Page = 1, PageSize = 5, OrderBy = "email desc", };
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
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

    [Fact]
    public async Task QueryUsersAsync_ServiceAreaDoesNotExists_ShouldThrow()
    {
        // Arrange
        TestData.TestServiceArea.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(1));
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var task = _serviceAreaService.QueryUsersAsync(new Guid());

        // Arrange
        task.Exception!.InnerException.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public async Task QueryUsersAsync_ServiceAreaExists_ShouldReturnUsersInDescendingOrderByEmail()
    {
        // Arrange
        TestData.TestServiceArea.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(3));
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var query = await _serviceAreaService.QueryUsersAsync(serviceArea.Id);
        var listOfUsers = query
            .OrderByDescending(u => u.Email)
            .ToArray();

        // Arrange
        using (new AssertionScope())
        {
            listOfUsers.Length.Should().Be(3);
            listOfUsers.Select(x => x.Email).Should().BeInDescendingOrder();
        }
    }

    [Fact]
    public async Task QueryServiceAreas_ReturnsJobTitles()
    {
        // Arrange
        var items = TestData.TestServiceArea.Generate(5);
        await _dbContextMock.ServiceAreas.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var query = _serviceAreaService.QueryServiceAreas();
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

        public static readonly Faker<ServiceArea> TestServiceArea =
            new Faker<ServiceArea>()
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

        public static readonly Faker<UpdateServiceAreaDto> UpdateServiceAreaDtoFaker =
            new Faker<UpdateServiceAreaDto>()
                .RuleFor(dto => dto.Name, f => f.Company.CompanyName())
                .RuleFor(dto => dto.Description, f => f.Lorem.Text());
    }
}

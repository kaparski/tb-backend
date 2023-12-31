﻿using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics.CodeAnalysis;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Exceptions;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.Administration.Divisions;
using TaxBeacon.Administration.Divisions.Activities.Factories;
using TaxBeacon.Administration.Divisions.Models;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services;

public class DivisionsServiceTests
{
    private readonly DivisionsService _divisionsService;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<IListToFileConverter> _csvMock;
    private readonly Mock<IListToFileConverter> _xlsxMock;
    private readonly Mock<ILogger<DivisionsService>> _divisionsServiceLoggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IDateTimeFormatter> _dateTimeFormatterMock;
    private readonly Mock<IEnumerable<IListToFileConverter>> _listToFileConverters;
    private readonly Mock<IDivisionActivityFactory> _divisionActivityFactory;
    private readonly Mock<IEnumerable<IDivisionActivityFactory>> _activityFactories;

    private readonly User _currentUser = TestData.TestUser.Generate();
    private static readonly Guid TenantId = Guid.NewGuid();
    public DivisionsServiceTests()
    {
        _entitySaveChangesInterceptorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(TeamServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _divisionsServiceLoggerMock = new();
        _dateTimeServiceMock = new();
        _csvMock = new();
        _xlsxMock = new();
        _dateTimeFormatterMock = new();
        _listToFileConverters = new();

        _csvMock.Setup(x => x.FileType).Returns(FileType.Csv);
        _xlsxMock.Setup(x => x.FileType).Returns(FileType.Xlsx);

        _listToFileConverters
            .Setup(x => x.GetEnumerator())
            .Returns(new[]
                {
                    _csvMock.Object, _xlsxMock.Object
                }.ToList()
                .GetEnumerator());
        _currentUserServiceMock = new();
        _dbContextMock.Tenants.Add(new Tenant()
        {
            Id = TenantId,
            Name = "TestTenant",
        });
        _dbContextMock.SaveChangesAsync();
        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TenantId);

        _divisionActivityFactory = new();
        _divisionActivityFactory.Setup(x => x.EventType).Returns(DivisionEventType.None);
        _divisionActivityFactory.Setup(x => x.Revision).Returns(1);

        _activityFactories = new();
        _activityFactories
            .Setup(x => x.GetEnumerator())
            .Returns(new[] { _divisionActivityFactory.Object }.ToList().GetEnumerator());
        _divisionsService = new DivisionsService(_divisionsServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            _listToFileConverters.Object,
            _dateTimeFormatterMock.Object,
            _activityFactories.Object);

        var currentUser = TestData.TestUser.Generate();
        _dbContextMock.Users.Add(currentUser);
        _dbContextMock.SaveChangesAsync().Wait();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        _activityFactories
            .Setup(x => x.GetEnumerator())
            .Returns(new IDivisionActivityFactory[]
            {
                new DivisionUpdatedEventFactory(),
            }.ToList().GetEnumerator());
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportDivisionsAsync_ValidInputData_AppropriateConverterShouldBeCalled(FileType fileType)
    {
        //Arrange
        var teams = TestData.TestDivision.Generate(5);

        await _dbContextMock.Divisions.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();

        //Act
        _ = await _divisionsService.ExportDivisionsAsync(fileType, default);

        //Assert
        if (fileType == FileType.Csv)
        {
            _csvMock.Verify(x => x.Convert(It.IsAny<List<DivisionExportModel>>()), Times.Once());
        }
        else if (fileType == FileType.Xlsx)
        {
            _xlsxMock.Verify(x => x.Convert(It.IsAny<List<DivisionExportModel>>()), Times.Once());
        }
        else
        {
            throw new InvalidOperationException();
        }
    }

    [Fact]
    public async Task GetActivitiesAsync_DivisionExists_ShouldCallAppropriateFactory()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;

        var divisionActivity = new DivisionActivityLog()
        {
            Date = DateTime.UtcNow,
            TenantId = TenantId,
            Division = division,
            EventType = DivisionEventType.None,
            Revision = 1
        };

        _dbContextMock.DivisionActivityLogs.Add(divisionActivity);
        await _dbContextMock.SaveChangesAsync();

        //Act
        await _divisionsService.GetActivitiesAsync(division.Id);

        //Assert

        _divisionActivityFactory.Verify(x => x.Create(It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetActivitiesAsync_DivisionDoesNotExistWithinCurrentTenant_ShouldReturnNotFound()
    {
        //Arrange
        var tenant = TestData.TestTenant.Generate();
        var division = TestData.TestDivision.Generate();
        division.Tenant = tenant;
        _dbContextMock.Tenants.Add(tenant);
        _dbContextMock.Divisions.Add(division);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _divisionsService.GetActivitiesAsync(division.Id);

        //Assert
        resultOneOf.TryPickT1(out _, out _).Should().BeTrue();
    }

    [Fact]
    public async Task GetActivitiesAsync_DivisionExists_ShouldReturnExpectedNumberOfItems()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;

        var activities = new[]
        {
            new DivisionActivityLog()
            {
                Date = new DateTime(2000, 01, 1),
                TenantId = TenantId,
                Division = division,
                EventType = DivisionEventType.None,
                Revision = 1
            },
        };

        _dbContextMock.DivisionActivityLogs.AddRange(activities);
        await _dbContextMock.SaveChangesAsync();

        const int pageSize = 2;

        //Act
        var resultOneOf = await _divisionsService.GetActivitiesAsync(division.Id, 1, pageSize);

        //Assert
        using (new AssertionScope())
        {
            resultOneOf.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(1);
        }
    }

    [Fact]
    public async Task GetDivisionDetailsAsync_ValidId_ReturnsDivision()
    {
        //Arrange
        TestData.TestDivision.RuleFor(
            x => x.TenantId, _ => TenantId);
        var divisions = TestData.TestDivision.Generate(5);

        await _dbContextMock.Divisions.AddRangeAsync(divisions);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var result = await _divisionsService.GetDivisionDetailsAsync(divisions[0].Id);

        //Assert
        using (new AssertionScope())
        {
            result.TryPickT0(out var divisionDetails, out _).Should().BeTrue();
            divisionDetails.Id.Should().Be(divisions[0].Id);
        }
    }

    [Fact]
    public async Task GetDivisionDetailsAsync_IdNotInDb_ReturnsNotFound()
    {
        //Arrange
        TestData.TestDivision.RuleFor(
            x => x.TenantId, _ => TenantId);
        var divisions = TestData.TestDivision.Generate(5);

        await _dbContextMock.Divisions.AddRangeAsync(divisions);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var result = await _divisionsService.GetDivisionDetailsAsync(new Guid());

        //Assert
        result.TryPickT1(out _, out _).Should().BeTrue();
    }

    [Fact]
    public async Task QueryDivisionUsersAsync_DivisionExistsAndFilterApplied_ShouldReturnUsersWithSpecificDepartment()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;
        var listOfUsers = TestData.TestUser.Generate(5);
        division.Users = listOfUsers;
        await _dbContextMock.Divisions.AddRangeAsync(division);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var query = await _divisionsService.QueryDivisionUsersAsync(division.Id);

        var departmentName = listOfUsers.First().Department!.Name;

        var users = query
            .Where(u => u.Department == departmentName)
            .OrderBy(u => u.Department)
            .ToArray();

        //Assert
        using (new AssertionScope())
        {
            users.Length.Should().BeGreaterThan(0);
            users.Should().BeInAscendingOrder((o1, o2) => string.Compare(o1.Department, o2.Department, StringComparison.InvariantCultureIgnoreCase));
            users.Should().AllSatisfy(u => u.Department.Should().Be(users.First().Department));
        }
    }

    [Fact]
    public async Task QueryDivisionUsersAsync_DivisionDoesNotExist_ShouldThrow()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;
        await _dbContextMock.Divisions.AddRangeAsync(division);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var task = _divisionsService.QueryDivisionUsersAsync(new Guid());

        //Assert
        task.Exception!.InnerException.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public async Task UpdateDivisionAsync_DivisionExists_ReturnsUpdatedDivisionAndCapturesActivityLog()
    {
        // Arrange
        var updateDivisionDto = TestData.UpdateDivisionDtoFaker.Generate();
        var division = TestData.TestDivision.Generate();
        await _dbContextMock.Divisions.AddAsync(division);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;
        _dateTimeServiceMock
            .Setup(service => service.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _divisionsService.UpdateDivisionAsync(division.Id, updateDivisionDto, default);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var divisionDto, out _);
            divisionDto.Should().NotBeNull();
            divisionDto.Id.Should().Be(division.Id);
            divisionDto.Name.Should().Be(updateDivisionDto.Name);

            var actualActivityLog = await _dbContextMock.DivisionActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(DivisionEventType.DivisionUpdatedEvent);
            actualActivityLog?.TenantId.Should().Be(TenantId);
            actualActivityLog?.DivisionId.Should().Be(division.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task GetDivisionDepartmentsAsync_DivisionExists_ShouldReturnDivisionDepartments()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;
        var departments = TestData.TestDepartment.Generate(5);
        division.Departments = departments;
        await _dbContextMock.Divisions.AddRangeAsync(division);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _divisionsService.GetDivisionDepartmentsAsync(division.Id);

        //Assert
        using (new AssertionScope())
        {
            resultOneOf.TryPickT0(out var divisionDepartmentDtos, out _).Should().BeTrue();
            divisionDepartmentDtos.Should().AllBeOfType<DivisionDepartmentDto>();
            divisionDepartmentDtos.Length.Should().Be(5);
        }
    }

    [Fact]
    public async Task GetDivisionDepartmentsAsync_DivisionDoesNotExist_ShouldReturnNotFound()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;
        var departments = TestData.TestDepartment.Generate(5);
        division.Departments = departments;
        await _dbContextMock.Divisions.AddRangeAsync(division);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var resultOneOf = await _divisionsService.GetDivisionDepartmentsAsync(Guid.NewGuid());

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task GetDivisionDepartmentsAsync_UserIsFromDifferentTenant_ShouldReturnNotFound()
    {
        //Arrange
        var division = TestData.TestDivision.Generate();
        division.TenantId = TenantId;
        var departments = TestData.TestDepartment.Generate(5);
        division.Departments = departments;
        await _dbContextMock.Divisions.AddRangeAsync(division);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(Guid.NewGuid());

        //Act
        var resultOneOf = await _divisionsService.GetDivisionDepartmentsAsync(division.Id);

        //Assert
        resultOneOf.IsT0.Should().BeFalse();
        resultOneOf.IsT1.Should().BeTrue();
    }

    [Fact]
    public async Task QueryDivisions_ReturnsDivisions()
    {
        // Arrange
        var items = TestData.TestDivision.Generate(5);
        await _dbContextMock.Divisions.AddRangeAsync(items);
        await _dbContextMock.SaveChangesAsync();

        // Act
        var query = _divisionsService.QueryDivisions();
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
        public static readonly Faker<User> TestUser =
            new Faker<User>()
                .RuleFor(u => u.Id, _ => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.FullName, (_, u) => $"{u.FirstName} {u.LastName}")
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>())
                .RuleFor(u => u.JobTitle, f =>
                    new JobTitle
                    {
                        Name = f.Name.FirstName()
                    })
                .RuleFor(u => u.Department, f =>
                    new Department
                    {
                        Name = f.Name.FirstName()
                    })
                .RuleFor(u => u.TenantUsers, (f, u) =>
                    new List<TenantUser>()
                    {
                        new TenantUser()
                        {
                            UserId = u.Id,
                            TenantId = TenantId
                        }
                    });

        public static readonly Faker<Division> TestDivision =
            new Faker<Division>()
                .RuleFor(d => d.Id, _ => Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Name.JobType())
                .RuleFor(d => d.CreatedDateTimeUtc, _ => DateTime.UtcNow)
                .RuleFor(d => d.Description, f => f.Lorem.Sentence(2))
                .RuleFor(d => d.TenantId, _ => DivisionsServiceTests.TenantId);

        public static readonly Faker<Tenant> TestTenant =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static readonly Faker<UpdateDivisionDto> UpdateDivisionDtoFaker =
            new Faker<UpdateDivisionDto>()
                .RuleFor(t => t.Name, f => f.Name.JobType())
                .RuleFor(t => t.Description, f => f.Lorem.Sentence(2))
                .RuleFor(t => t.DepartmentIds, f => f.Make(3, Guid.NewGuid));

        public static readonly Faker<Department> TestDepartment =
            new Faker<Department>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.TenantId, _ => DivisionsServiceTests.TenantId)
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Description, f => f.Lorem.Sentence(2))
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);
    }
}

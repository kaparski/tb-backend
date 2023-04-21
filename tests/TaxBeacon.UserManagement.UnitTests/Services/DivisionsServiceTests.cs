using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Activities;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;
using TaxBeacon.UserManagement.Services.Activities.Divisions;

namespace TaxBeacon.UserManagement.UnitTests.Services
{
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
        public static readonly Guid TenantId = Guid.NewGuid();
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
        }
        [Fact]
        public async Task GetDivisionsAsync_AscOrderingAndPaginationOfLastPage_AscOrderOfDivisionsAndCorrectPage()
        {
            // Arrange
            var divisions = TestData.TestDivision.Generate(5);
            _currentUser.Division = divisions[0];
            await _dbContextMock.Divisions.AddRangeAsync(divisions);
            await _dbContextMock.SaveChangesAsync();
            var query = new GridifyQuery
            {
                Page = 1,
                PageSize = 10,
                OrderBy = "name asc"
            };

            // Act
            var divisionsOneOf = await _divisionsService.GetDivisionsAsync(query, default);

            // Assert
            using (new AssertionScope())
            {
                divisionsOneOf.TryPickT0(out var pageOfDivisions, out _);
                pageOfDivisions.Should().NotBeNull();
                var listOfDivisions = pageOfDivisions.Query.ToList();
                listOfDivisions.Count.Should().Be(5);
                listOfDivisions.Select(x => x.Name).Should().BeInAscendingOrder();
                pageOfDivisions.Count.Should().Be(5);
            }
        }

        [Fact]
        public async Task GetDivisionsAsync_DescOrderingAndPaginationWithFirstPage_CorrectNumberOfDivisionsInDescOrder()
        {
            // Arrange
            var divisions = TestData.TestDivision.Generate(7);
            await _dbContextMock.Divisions.AddRangeAsync(divisions);
            await _dbContextMock.SaveChangesAsync();
            var query = new GridifyQuery
            {
                Page = 1,
                PageSize = 4,
                OrderBy = "name desc"
            };

            // Act
            var divisionsOneOf = await _divisionsService.GetDivisionsAsync(query, default);

            // Assert
            using (new AssertionScope())
            {
                divisionsOneOf.TryPickT0(out var pageOfDivisions, out _);
                pageOfDivisions.Should().NotBeNull();
                var listOfDivisions = pageOfDivisions.Query.ToList();
                listOfDivisions.Count.Should().Be(4);
                listOfDivisions.Select(x => x.Name).Should().BeInDescendingOrder();
                pageOfDivisions.Count.Should().Be(7);
            }
        }

        [Fact]
        public async Task GetDivisionsAsync_NoDivisions_NumberOfDivisionsEmpty()
        {
            // Arrange
            var query = new GridifyQuery
            {
                Page = 1,
                PageSize = 123,
                OrderBy = "name desc"
            };

            // Act
            var divisionsOneOf = await _divisionsService.GetDivisionsAsync(query, default);

            // Assert
            using (new AssertionScope())
            {
                divisionsOneOf.TryPickT0(out var pageOfDivisions, out _);
                pageOfDivisions.Should().NotBeNull();
                var listOfDivisions = pageOfDivisions.Query.ToList();
                listOfDivisions.Count.Should().Be(0);
                pageOfDivisions.Count.Should().Be(0);
            }
        }

        [Theory]
        [InlineData(7, 25, 2)]
        [InlineData(10, 5, 3)]
        public async Task GetDivisionsAsync_PageNumberOutsideOfTotalRange_DivisionListIsEmpty(int numberOfDivisions, int pageSize, int pageNumber)
        {
            // Arrange
            var divisions = TestData.TestDivision.Generate(numberOfDivisions);
            await _dbContextMock.Divisions.AddRangeAsync(divisions);
            await _dbContextMock.SaveChangesAsync();
            var query = new GridifyQuery
            {
                Page = pageNumber,
                PageSize = pageSize,
                OrderBy = "name asc",
            };

            // Act
            var divisionsOneOf = await _divisionsService.GetDivisionsAsync(query, default);

            // Assert
            divisionsOneOf.TryPickT0(out var pageOfDivisions, out _);
            pageOfDivisions.Should().BeNull();
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
                    .RuleFor(u => u.Status, f => f.PickRandom<Status>());

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
        }
    }
}

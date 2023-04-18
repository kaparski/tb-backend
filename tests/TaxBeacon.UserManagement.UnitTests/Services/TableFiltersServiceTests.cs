using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.UserManagement.UnitTests.Services;

public class TableFiltersServiceTests
{
    private readonly Mock<IDateTimeService> _dateTimeServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock;
    private readonly Mock<ILogger<TableFilterService>> _tableFiltersServiceLoggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ITaxBeaconDbContext _dbContextMock;
    private readonly ITableFiltersService _tableFiltersService;

    public TableFiltersServiceTests()
    {
        _dateTimeServiceMock = new();
        _entitySaveChangesInterceptorMock = new();
        _tableFiltersServiceLoggerMock = new();
        _currentUserServiceMock = new();

        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UserServiceTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _tableFiltersService = new TableFilterService(
            _tableFiltersServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object);
    }

    [Theory]
    [InlineData(EntityType.User)]
    [InlineData(EntityType.Role)]
    [InlineData(EntityType.Tenant)]
    [InlineData(EntityType.Team)]
    [InlineData(EntityType.TenantDivision)]
    public async Task GetFiltersAsync_TableType_ReturnsCollectionOfTableFilters(EntityType tableType)
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var users = TestData.UserFaker.Generate(2);
        var firstUserFilters = TestData.TableFilterFaker
            .RuleFor(tf => tf.UserId, _ => users[0].Id)
            .RuleFor(tf => tf.TenantId, _ => tenants[0].Id)
            .Generate(5);

        var secondUserFilters = TestData.TableFilterFaker
            .RuleFor(tf => tf.UserId, _ => users[^1].Id)
            .RuleFor(tf => tf.TenantId, _ => tenants[^1].Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TableFilters.AddRangeAsync(firstUserFilters);
        await _dbContextMock.TableFilters.AddRangeAsync(secondUserFilters);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[0].Id);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(users[0].Id);

        var expectedFilters = firstUserFilters
            .Where(tf => tf.TableType == tableType)
            .ToList();

        // Act
        var actualResult = await _tableFiltersService.GetFiltersAsync(tableType);

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveSameCount(expectedFilters);
            actualResult.Select(dto => dto.Id).Should().Equal(expectedFilters.Select(tf => tf.Id));
        }
    }

    [Fact]
    public async Task GetFiltersAsync_UserWithoutFilters_ReturnsEmptyCollection()
    {
        // Arrange
        var tableType = new Faker().PickRandom<EntityType>();
        var tenants = TestData.TenantFaker.Generate(2);
        var users = TestData.UserFaker.Generate(2);
        var firstUserFilters = TestData.TableFilterFaker
            .RuleFor(tf => tf.UserId, _ => users[0].Id)
            .RuleFor(tf => tf.TenantId, _ => tenants[0].Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TableFilters.AddRangeAsync(firstUserFilters);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[^1].Id);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(users[^1].Id);

        // Act
        var actualResult = await _tableFiltersService.GetFiltersAsync(tableType);

        // Assert
        actualResult.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetFiltersAsync_UserWithoutTenant_ReturnsCollectionOfTableFilters()
    {
        // Arrange
        var tableType = new Faker().PickRandom<EntityType>();
        var tenants = TestData.TenantFaker.Generate(2);
        var users = TestData.UserFaker.Generate(2);
        var firstUserFilters = TestData.TableFilterFaker
            .RuleFor(tf => tf.UserId, _ => users[0].Id)
            .RuleFor(tf => tf.TenantId, _ => null)
            .Generate(5);

        var secondUserFilters = TestData.TableFilterFaker
            .RuleFor(tf => tf.UserId, _ => users[^1].Id)
            .RuleFor(tf => tf.TenantId, _ => tenants[^1].Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TableFilters.AddRangeAsync(firstUserFilters);
        await _dbContextMock.TableFilters.AddRangeAsync(secondUserFilters);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(Guid.Empty);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(users[0].Id);

        var expectedFilters = firstUserFilters
            .Where(tf => tf.TableType == tableType)
            .ToList();

        // Act
        var actualResult = await _tableFiltersService.GetFiltersAsync(tableType);

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveSameCount(expectedFilters);
            actualResult.Select(dto => dto.Id).Should().Equal(expectedFilters.Select(tf => tf.Id));
        }
    }

    [Fact]
    public async Task CreateFilterAsync_ValidCreateTableFilterDto_ReturnsCollectionOfTableFilters()
    {
        // Arrange
        var createTableFilterDto = TestData.CreateTableFilterDtoFaker.Generate();
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        // Act
        var createdTableFilterOneOf = await _tableFiltersService.CreateFilterAsync(createTableFilterDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);

            createdTableFilterOneOf.IsT0.Should().BeTrue();
            createdTableFilterOneOf.IsT1.Should().BeFalse();
            createdTableFilterOneOf.TryPickT0(out var tableFilters, out _);
            tableFilters.Should().NotBeNullOrEmpty();
            tableFilters.Should().Contain(tf =>
                tf.Name.Equals(createTableFilterDto.Name, StringComparison.OrdinalIgnoreCase));

            var actualDbTableFilter = await _dbContextMock.TableFilters.LastOrDefaultAsync();
            actualDbTableFilter.Should().NotBeNull();
            actualDbTableFilter?.TableType.Should().Be(createTableFilterDto.TableType);
            actualDbTableFilter?.Name.Should().Be(createTableFilterDto.Name);
            actualDbTableFilter?.Configuration.Should().Be(createTableFilterDto.Configuration);
            actualDbTableFilter?.UserId.Should().Be(user.Id);
            actualDbTableFilter?.TenantId.Should().Be(tenant.Id);
        }
    }

    [Fact]
    public async Task CreateFilterAsync_ValidCreateTableFilterDtoAndUserWithoutTenant_ReturnsCollectionOfTableFilters()
    {
        // Arrange
        var createTableFilterDto = TestData.CreateTableFilterDtoFaker.Generate();
        var user = TestData.UserFaker.Generate();
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(Guid.Empty);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        // Act
        var createdTableFilterOneOf = await _tableFiltersService.CreateFilterAsync(createTableFilterDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);

            createdTableFilterOneOf.IsT0.Should().BeTrue();
            createdTableFilterOneOf.IsT1.Should().BeFalse();
            createdTableFilterOneOf.TryPickT0(out var tableFilters, out _);
            tableFilters.Should().NotBeNullOrEmpty();
            tableFilters.Should().Contain(tf =>
                tf.Name.Equals(createTableFilterDto.Name, StringComparison.OrdinalIgnoreCase));

            var actualDbTableFilter = await _dbContextMock.TableFilters.LastOrDefaultAsync();
            actualDbTableFilter.Should().NotBeNull();
            actualDbTableFilter?.TableType.Should().Be(createTableFilterDto.TableType);
            actualDbTableFilter?.Name.Should().Be(createTableFilterDto.Name);
            actualDbTableFilter?.Configuration.Should().Be(createTableFilterDto.Configuration);
            actualDbTableFilter?.UserId.Should().Be(user.Id);
            actualDbTableFilter?.TenantId.Should().BeNull();
        }
    }

    [Fact]
    public async Task CreateFilterAsync_ValidCreateTableFilterDtoWithExistingFilterName_ReturnsNameAlreadyExists()
    {
        // Arrange
        var createTableFilterDto = TestData.CreateTableFilterDtoFaker.Generate();
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var tableFilter = createTableFilterDto.Adapt<TableFilter>();
        tableFilter.UserId = user.Id;
        tableFilter.TenantId = tenant.Id;

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TableFilters.AddAsync(tableFilter);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        // Act
        var createdTableFilterOneOf = await _tableFiltersService.CreateFilterAsync(createTableFilterDto);

        // Assert
        using (new AssertionScope())
        {
            createdTableFilterOneOf.IsT0.Should().BeFalse();
            createdTableFilterOneOf.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task DeleteFiltersAsync_FilterExistsInDb_ReturnsEmptyList()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var tableFilter = TestData.TableFilterFaker.Generate();
        tableFilter.UserId = user.Id;
        tableFilter.TenantId = tenant.Id;

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TableFilters.AddAsync(tableFilter);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        // Act
        var deleteResultOneOf = await _tableFiltersService.DeleteFilterAsync(tableFilter.Id);

        // Assert
        using (new AssertionScope())
        {
            deleteResultOneOf.IsT0.Should().BeTrue();
            deleteResultOneOf.IsT1.Should().BeFalse();
            deleteResultOneOf.TryPickT0(out var tableFilters, out _);
            tableFilters.Should().BeEmpty();

            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            (await _dbContextMock.TableFilters.AnyAsync(tf => tf.Id == tableFilter.Id)).Should().BeFalse();
        }
    }

    [Fact]
    public async Task DeleteFiltersAsync_FilterExistsInDbAndUserWithoutTenant_ReturnsEmptyList()
    {
        // Arrange
        var user = TestData.UserFaker.Generate();
        var tableFilter = TestData.TableFilterFaker.Generate();
        tableFilter.UserId = user.Id;

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.TableFilters.AddAsync(tableFilter);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(Guid.Empty);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        // Act
        var deleteResultOneOf = await _tableFiltersService.DeleteFilterAsync(tableFilter.Id);

        // Assert
        using (new AssertionScope())
        {
            deleteResultOneOf.IsT0.Should().BeTrue();
            deleteResultOneOf.IsT1.Should().BeFalse();
            deleteResultOneOf.TryPickT0(out var tableFilters, out _);
            tableFilters.Should().BeEmpty();

            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            (await _dbContextMock.TableFilters.AnyAsync(tf => tf.Id == tableFilter.Id)).Should().BeFalse();
        }
    }

    [Fact]
    public async Task DeleteFiltersAsync_FilterNotExistsInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var tableFilter = TestData.TableFilterFaker.Generate();
        tableFilter.UserId = user.Id;
        tableFilter.TenantId = tenant.Id;

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TableFilters.AddAsync(tableFilter);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        // Act
        var deleteResultOneOf = await _tableFiltersService.DeleteFilterAsync(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            deleteResultOneOf.IsT0.Should().BeFalse();
            deleteResultOneOf.IsT1.Should().BeTrue();
        }
    }

    private static class TestData
    {
        public static readonly Faker<Tenant> TenantFaker =
            new Faker<Tenant>()
                .RuleFor(t => t.Id, _ => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName());

        public static readonly Faker<TableFilter> TableFilterFaker =
            new Faker<TableFilter>()
                .RuleFor(tf => tf.Id, _ => Guid.NewGuid())
                .RuleFor(tf => tf.Name, f => f.Hacker.Noun())
                .RuleFor(tf => tf.Configuration, f => f.Lorem.Text())
                .RuleFor(tf => tf.TableType, f => f.PickRandom<EntityType>())
                .RuleFor(tf => tf.TenantId, _ => Guid.NewGuid())
                .RuleFor(tf => tf.UserId, _ => Guid.NewGuid());

        public static readonly Faker<User> UserFaker =
            new Faker<User>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.FirstName, f => f.Name.FirstName())
                .RuleFor(u => u.LastName, f => f.Name.LastName())
                .RuleFor(u => u.FullName, f => f.Name.FullName())
                .RuleFor(u => u.LegalName, (_, u) => u.FirstName)
                .RuleFor(u => u.Email, f => f.Internet.Email())
                .RuleFor(u => u.CreatedDateTimeUtc, f => DateTime.UtcNow)
                .RuleFor(u => u.Status, f => f.PickRandom<Status>());

        public static readonly Faker<CreateTableFilterDto> CreateTableFilterDtoFaker =
            new Faker<CreateTableFilterDto>()
                .RuleFor(dto => dto.Name, f => f.Hacker.Noun())
                .RuleFor(dto => dto.Configuration, f => f.Lorem.Text())
                .RuleFor(dto => dto.TableType, f => f.PickRandom<EntityType>());
    }
}

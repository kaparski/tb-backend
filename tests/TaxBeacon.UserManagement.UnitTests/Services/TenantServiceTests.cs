using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Entities;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

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
        _currentUserServiceMock.Setup(x => x.UserId).Returns(currentUser.Id);

        _tenantService = new TenantService(
            _tenantServiceLoggerMock.Object,
            _dbContextMock,
            _dateTimeServiceMock.Object,
            _currentUserServiceMock.Object,
            _listToFileConverters.Object,
            _dateTimeFormatterMock.Object);

        TypeAdapterConfig.GlobalSettings.Scan(typeof(UserMappingConfig).Assembly);
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
        var tenantsOneOf = await _tenantService.GetTenantsAsync(query, default);

        // Assert
        tenantsOneOf.TryPickT0(out var pageOfTenants, out _);
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
        var tenantsOneOf = await _tenantService.GetTenantsAsync(query, default);

        // Assert
        using (new AssertionScope())
        {
            tenantsOneOf.TryPickT0(out var pageOfTenants, out _);
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
        var tenantsOneOf = await _tenantService.GetTenantsAsync(query, default);

        // Assert
        using (new AssertionScope())
        {
            tenantsOneOf.TryPickT0(out var pageOfTenants, out _);
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
        var tenantsOneOf = await _tenantService.GetTenantsAsync(query, default);

        // Assert
        tenantsOneOf.TryPickT0(out var pageOfTenants, out _);
        pageOfTenants.Should().BeNull();
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
        var tenantsOneOf = await _tenantService.GetTenantsAsync(query, default);

        // Assert
        tenantsOneOf.TryPickT0(out var pageOfTenants, out _);
        pageOfTenants.Should().BeNull();
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
        var actualResult = await _tenantService.GetTenantByIdAsync(tenant.Id, default);

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
        var actualResult = await _tenantService.GetTenantByIdAsync(Guid.NewGuid(), default);

        // Assert
        actualResult.TryPickT0(out var _, out var notFound);
        notFound.Should().NotBeNull();
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
        var itemsOneOf = await _tenantService.GetDepartmentsAsync(TestData.TestTenantId, query, default);

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
        var itemsOneOf = await _tenantService.GetDepartmentsAsync(TestData.TestTenantId, query, default);

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
        var itemsOneOf = await _tenantService.GetDepartmentsAsync(TestData.TestTenantId, query, default);

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
        var itemsOneOf = await _tenantService.GetDepartmentsAsync(TestData.TestTenantId, query, default);

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
        var itemsOneOf = await _tenantService.GetDepartmentsAsync(TestData.TestTenantId, query, default);

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
        _ = await _tenantService.ExportDepartmentsAsync(TestData.TestTenantId, fileType, default);

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
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
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

        public static readonly Faker<UpdateUserDto> UpdateUserDtoFaker =
            new Faker<UpdateUserDto>()
                .RuleFor(dto => dto.FirstName, f => f.Name.FirstName())
                .RuleFor(dto => dto.LastName, f => f.Name.LastName());

        public static IEnumerable<object[]> UpdatedStatusInvalidData =>
            new List<object[]>
            {
                new object[] { Status.Active, Guid.NewGuid() },
                new object[] { Status.Deactivated, Guid.Empty }
            };

        public static readonly Faker<Role> TestRoles =
            new Faker<Role>()
                .RuleFor(u => u.Id, f => Guid.NewGuid())
                .RuleFor(u => u.Name, f => f.Name.JobTitle());
    }
}

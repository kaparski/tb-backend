using Bogus;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.API.Controllers.Departments.Requests;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.Departments.Request;

public class UpdateDepartmentRequestValidatorTest
{
    private readonly UpdateDepartmentRequestValidator _updateDepartmentRequestValidator;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public UpdateDepartmentRequestValidatorTest()
    {
        _currentUserServiceMock = new();
        _httpContextAccessorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UpdateDepartmentRequestValidatorTest)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _updateDepartmentRequestValidator = new UpdateDepartmentRequestValidator(_httpContextAccessorMock.Object, _dbContextMock, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var department = TestData.DepartmentFaker
            .RuleFor(d => d.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateDepartmentRequest = TestData.UpdateDepartmentRequestFaker
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", department.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateDepartmentRequestValidator.TestValidateAsync(updateDepartmentRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validation_EmptyName_ShouldHaveErrors(string name)
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var department = TestData.DepartmentFaker
            .RuleFor(d => d.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateDepartmentRequest = TestData.UpdateDepartmentRequestFaker
            .RuleFor(x => x.Name, name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", department.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateDepartmentRequestValidator.TestValidateAsync(updateDepartmentRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_LongName_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var department = TestData.DepartmentFaker
            .RuleFor(d => d.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateDepartmentRequest = TestData.UpdateDepartmentRequestFaker
            .RuleFor(u => u.Name, f => f.Random.String2(200))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", department.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateDepartmentRequestValidator.TestValidateAsync(updateDepartmentRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("The department name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_DepartmentWithTheSameNameAlreadyExists_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var departments = TestData.DepartmentFaker
            .RuleFor(d => d.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Departments.AddRangeAsync(departments);
        await _dbContextMock.SaveChangesAsync();

        var updateDepartmentRequest = TestData.UpdateDepartmentRequestFaker
            .RuleFor(r => r.Name, departments[1].Name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", departments[0].Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateDepartmentRequestValidator.TestValidateAsync(updateDepartmentRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("Department with the same name already exists");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_LongDescription_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var department = TestData.DepartmentFaker
            .RuleFor(d => d.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateDepartmentRequest = TestData.UpdateDepartmentRequestFaker
            .RuleFor(r => r.Description, f => f.Random.String2(201))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", department.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateDepartmentRequestValidator.TestValidateAsync(updateDepartmentRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldHaveValidationErrorFor(r => r.Description)
            .WithErrorMessage("The department description must contain no more than 200 characters");
    }

    private static class TestData
    {
        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static Faker<Department> DepartmentFaker =>
            new Faker<Department>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Description, f => f.Lorem.Sentence(2))
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static Faker<UpdateDepartmentRequest> UpdateDepartmentRequestFaker =>
            new Faker<UpdateDepartmentRequest>()
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.Description, f => f.Lorem.Sentence(2));

    }
}

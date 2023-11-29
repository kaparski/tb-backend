using Bogus;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.API.Controllers.ServiceAreas.Requests;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.ServiceAreas.Requests;

public class UpdateServiceAreaRequestValidatorTests
{
    private readonly UpdateServiceAreaRequestValidator _updateServiceAreaRequestValidator;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public UpdateServiceAreaRequestValidatorTests()
    {
        _currentUserServiceMock = new();
        _httpContextAccessorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UpdateServiceAreaRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _updateServiceAreaRequestValidator = new UpdateServiceAreaRequestValidator(_httpContextAccessorMock.Object, _dbContextMock, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var serviceArea = TestData.ServiceAreaFaker
            .RuleFor(s => s.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateServiceAreaRequest = TestData.UpdateServiceAreaRequestFaker
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", serviceArea.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateServiceAreaRequestValidator.TestValidateAsync(updateServiceAreaRequest);

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
        var serviceArea = TestData.ServiceAreaFaker
            .RuleFor(s => s.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateServiceAreaRequest = TestData.UpdateServiceAreaRequestFaker
            .RuleFor(x => x.Name, name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", serviceArea.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateServiceAreaRequestValidator.TestValidateAsync(updateServiceAreaRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_LongName_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var serviceArea = TestData.ServiceAreaFaker
            .RuleFor(s => s.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateServiceAreaRequest = TestData.UpdateServiceAreaRequestFaker
            .RuleFor(u => u.Name, f => f.Random.String2(200))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", serviceArea.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateServiceAreaRequestValidator.TestValidateAsync(updateServiceAreaRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("The service area name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_ServiceAreaWithTheSameNameAlreadyExists_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var serviceAreas = TestData.ServiceAreaFaker
            .RuleFor(s => s.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.ServiceAreas.AddRangeAsync(serviceAreas);
        await _dbContextMock.SaveChangesAsync();

        var updateServiceAreaRequest = TestData.UpdateServiceAreaRequestFaker
            .RuleFor(r => r.Name, serviceAreas[1].Name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", serviceAreas[0].Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateServiceAreaRequestValidator.TestValidateAsync(updateServiceAreaRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("Service area with the same name already exists");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_LongDescription_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var serviceArea = TestData.ServiceAreaFaker
            .RuleFor(s => s.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateServiceAreaRequest = TestData.UpdateServiceAreaRequestFaker
            .RuleFor(r => r.Description, f => f.Random.String2(201))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", serviceArea.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateServiceAreaRequestValidator.TestValidateAsync(updateServiceAreaRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldHaveValidationErrorFor(r => r.Description)
            .WithErrorMessage("The service area description must contain no more than 200 characters");
    }

    private static class TestData
    {
        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static Faker<ServiceArea> ServiceAreaFaker =>
            new Faker<ServiceArea>()
                .RuleFor(s => s.Id, f => Guid.NewGuid())
                .RuleFor(s => s.Name, f => f.Company.CompanyName())
                .RuleFor(s => s.Description, f => f.Lorem.Sentence(2))
                .RuleFor(s => s.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static Faker<UpdateServiceAreaRequest> UpdateServiceAreaRequestFaker =>
            new Faker<UpdateServiceAreaRequest>()
                .RuleFor(u => u.Name, f => f.Company.CompanyName())
                .RuleFor(u => u.Description, f => f.Lorem.Sentence(2));

    }
}

using Bogus;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.API.Controllers.Tenants.Requests;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.Tenants.Requests;

public class UpdateTenantRequestValidatorTests
{
    private readonly UpdateTenantRequestValidator _updateTenantRequestValidator;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public UpdateTenantRequestValidatorTests()
    {
        _httpContextAccessorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UpdateTenantRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _updateTenantRequestValidator = new UpdateTenantRequestValidator(_httpContextAccessorMock.Object, _dbContextMock);
    }

    [Fact]
    public async Task Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateTenantRequest = TestData.UpdateTenantRequestFaker
            .Generate();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", tenant.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateTenantRequestValidator.TestValidateAsync(updateTenantRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validation_EmptyName_ShouldHaveErrors(string name)
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateTenantRequest = TestData.UpdateTenantRequestFaker
            .RuleFor(x => x.Name, name)
            .Generate();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", tenant.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateTenantRequestValidator.TestValidateAsync(updateTenantRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Fact]
    public async Task Validation_LongName_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateTenantRequest = TestData.UpdateTenantRequestFaker
            .RuleFor(u => u.Name, f => f.Random.String2(101))
            .Generate();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", tenant.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateTenantRequestValidator.TestValidateAsync(updateTenantRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("The tenant name must contain no more than 100 characters");
    }

    [Fact]
    public async Task Validation_TenantWithTheSameNameAlreadyExists_ShouldHaveErrors()
    {
        //Arrange
        var tenants = TestData.TenantFaker
            .Generate(2);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.SaveChangesAsync();

        var updateTenantRequest = TestData.UpdateTenantRequestFaker
            .RuleFor(r => r.Name, tenants[1].Name)
            .Generate();

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", tenants[0].Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateTenantRequestValidator.TestValidateAsync(updateTenantRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("Tenant with the same name already exists");
    }

    private static class TestData
    {
        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);
        public static Faker<UpdateTenantRequest> UpdateTenantRequestFaker =>
            new Faker<UpdateTenantRequest>()
                .RuleFor(u => u.Name, f => f.Company.CompanyName());

    }
}

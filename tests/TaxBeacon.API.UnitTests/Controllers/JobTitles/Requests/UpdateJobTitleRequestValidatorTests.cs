using Bogus;
using Bogus.Extensions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.API.Controllers.JobTitles.Requests;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.JobTitles.Requests;

public class UpdateJobRequestValidatorTests
{
    private readonly UpdateJobTitleRequestValidator _updateJobTitleRequestValidator;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public UpdateJobRequestValidatorTests()
    {
        _currentUserServiceMock = new();
        _httpContextAccessorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UpdateJobRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _updateJobTitleRequestValidator = new UpdateJobTitleRequestValidator(_httpContextAccessorMock.Object, _dbContextMock, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var jobTitle = TestData.JobTitleFaker
            .RuleFor(j => j.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateJobTitleRequest = TestData.UpdateJobTitleRequestFaker
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", jobTitle.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateJobTitleRequestValidator.TestValidateAsync(updateJobTitleRequest);

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
        var jobTitle = TestData.JobTitleFaker
            .RuleFor(j => j.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateJobTitleRequest = TestData.UpdateJobTitleRequestFaker
            .RuleFor(u => u.Name, name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", jobTitle.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateJobTitleRequestValidator.TestValidateAsync(updateJobTitleRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_LongName_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var jobTitle = TestData.JobTitleFaker
            .RuleFor(j => j.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateJobTitleRequest = TestData.UpdateJobTitleRequestFaker
            .RuleFor(u => u.Name, f => f.Random.String2(200))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", jobTitle.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateJobTitleRequestValidator.TestValidateAsync(updateJobTitleRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("The job title name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_JobTitleWithTheSameNameAlreadyExists_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var jobTitles = TestData.JobTitleFaker
            .RuleFor(j => j.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.JobTitles.AddRangeAsync(jobTitles);
        await _dbContextMock.SaveChangesAsync();

        var updateJobTitleRequest = TestData.UpdateJobTitleRequestFaker
            .RuleFor(u => u.Name, jobTitles[1].Name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", jobTitles[0].Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateJobTitleRequestValidator.TestValidateAsync(updateJobTitleRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("Job title with the same name already exists");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_LongDescription_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var jobTitle = TestData.JobTitleFaker
            .RuleFor(j => j.TenantId, _ => tenant.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var updateJobTitleRequest = TestData.UpdateJobTitleRequestFaker
            .RuleFor(u => u.Description, f => f.Random.String2(201))
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", jobTitle.Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateJobTitleRequestValidator.TestValidateAsync(updateJobTitleRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldHaveValidationErrorFor(r => r.Description)
            .WithErrorMessage("The job title description must contain no more than 200 characters");
    }

    private static class TestData
    {
        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static Faker<JobTitle> JobTitleFaker =>
            new Faker<JobTitle>()
                .RuleFor(j => j.Id, f => Guid.NewGuid())
                .RuleFor(j => j.Name, f => f.Company.CompanyName())
                .RuleFor(j => j.Description, f => f.Lorem.Sentence(2))
                .RuleFor(j => j.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static Faker<UpdateJobTitleRequest> UpdateJobTitleRequestFaker =>
            new Faker<UpdateJobTitleRequest>()
                .RuleFor(u => u.Name, f => f.Company.CompanyName())
                .RuleFor(u => u.Description, f => f.Lorem.Sentence(2));
    }
}
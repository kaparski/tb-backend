using Bogus;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.API.Controllers.Divisions.Requests;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.Divisions.Requests;

public class UpdateDivisionRequestValidatorTests
{
    private readonly UpdateDivisionRequestValidator _updateDivisionRequestValidator;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;

    public UpdateDivisionRequestValidatorTests()
    {
        _currentUserServiceMock = new();
        _httpContextAccessorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UpdateDivisionRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);

        _updateDivisionRequestValidator = new UpdateDivisionRequestValidator(_httpContextAccessorMock.Object, _dbContextMock, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var updateDivisionRequest = TestData.UpdateDivisionRequestFaker
            .Generate();

        //Act
        var actualResult = await _updateDivisionRequestValidator.TestValidateAsync(updateDivisionRequest);

        //Assert
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task Validation_InvalidName_ShouldHaveErrors(string name)
    {
        //Arrange
        var updateDivisionRequest = TestData.UpdateDivisionRequestFaker
            .RuleFor(u => u.Name, name)
            .Generate();

        //Act
        var actualResult = await _updateDivisionRequestValidator.TestValidateAsync(updateDivisionRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_LongName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var name = new Faker().Random.String2(115);
        var updateDivisionRequest = TestData.UpdateDivisionRequestFaker
            .RuleFor(u => u.Name, name)
            .Generate();

        //Act
        var actualResult = await _updateDivisionRequestValidator.TestValidateAsync(updateDivisionRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("The division name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_DivisionWithTheSameNameAlreadyExists_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var divisions = TestData.DivisionFaker
            .RuleFor(d => d.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Divisions.AddRangeAsync(divisions);
        await _dbContextMock.SaveChangesAsync();

        var updateDivisionRequest = TestData.UpdateDivisionRequestFaker
            .RuleFor(u => u.Name, divisions[1].Name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", divisions[0].Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateDivisionRequestValidator.TestValidateAsync(updateDivisionRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("Division with the same name already exists");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_LongDescription_ShouldHaveMaxLengthError()
    {
        //Arrange
        var description = new Faker().Random.String2(270);
        var updateDivisionRequest = TestData.UpdateDivisionRequestFaker
            .RuleFor(u => u.Description, description)
            .Generate();

        //Act
        var actualResult = await _updateDivisionRequestValidator.TestValidateAsync(updateDivisionRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Description)
            .WithErrorMessage("The division description must contain no more than 200 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
    }

    private static class TestData
    {
        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static Faker<Division> DivisionFaker =>
            new Faker<Division>()
                .RuleFor(d => d.Id, f => Guid.NewGuid())
                .RuleFor(d => d.Name, f => f.Company.CompanyName())
                .RuleFor(d => d.Description, f => f.Lorem.Sentence(2))
                .RuleFor(d => d.CreatedDateTimeUtc, _ => DateTime.UtcNow);

        public static Faker<UpdateDivisionRequest> UpdateDivisionRequestFaker =>
            new Faker<UpdateDivisionRequest>()
                .RuleFor(u => u.Name, f => f.Company.CompanyName())
                .RuleFor(u => u.Description, f => f.Lorem.Sentence(2));

    }
}
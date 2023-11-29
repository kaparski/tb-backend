using Bogus;
using Bogus.Extensions;
using FluentValidation.TestHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.API.Controllers.Teams.Requests;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Administration.Entities;
using TaxBeacon.DAL.Interceptors;

namespace TaxBeacon.API.UnitTests.Controllers.Teams.Requests;

public class UpdateTeamRequestValidatorTests
{
    private readonly UpdateTeamRequestValidator _updateTeamRequestValidator;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly TaxBeaconDbContext _dbContextMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<EntitySaveChangesInterceptor> _entitySaveChangesInterceptorMock = new();

    public UpdateTeamRequestValidatorTests()
    {
        _currentUserServiceMock = new();
        _httpContextAccessorMock = new();
        _dbContextMock = new TaxBeaconDbContext(
            new DbContextOptionsBuilder<TaxBeaconDbContext>()
                .UseInMemoryDatabase($"{nameof(UpdateTeamRequestValidatorTests)}-InMemoryDb-{Guid.NewGuid()}")
                .Options,
            _entitySaveChangesInterceptorMock.Object);
        _updateTeamRequestValidator = new UpdateTeamRequestValidator(_httpContextAccessorMock.Object, _dbContextMock, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Validation_ValidRequest_ShouldHaveNoErrors()
    {
        //Arrange
        var updateTeamRequest = TestData.UpdateTeamFaker
            .Generate();

        //Act
        var actualResult = await _updateTeamRequestValidator.TestValidateAsync(updateTeamRequest);

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
        var updateTeamRequest = TestData.UpdateTeamFaker
            .RuleFor(u => u.Name, name)
            .Generate();

        //Act
        var actualResult = await _updateTeamRequestValidator.TestValidateAsync(updateTeamRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name);
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_LongName_ShouldHaveMaxLengthError()
    {
        //Arrange
        var name = new Faker().Random.String2(115);
        var updateTeamRequest = TestData.UpdateTeamFaker
            .RuleFor(u => u.Name, name)
            .Generate();

        //Act
        var actualResult = await _updateTeamRequestValidator.TestValidateAsync(updateTeamRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("The team name must contain no more than 100 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    [Fact]
    public async Task Validation_LongDescription_ShouldHaveMaxLengthError()
    {
        //Arrange
        var description = new Faker().Random.String2(270);
        var updateTeamRequest = TestData.UpdateTeamFaker
            .RuleFor(u => u.Description, description)
            .Generate();

        //Act
        var actualResult = await _updateTeamRequestValidator.TestValidateAsync(updateTeamRequest);

        //Assert
        actualResult
            .ShouldHaveValidationErrorFor(r => r.Description)
            .WithErrorMessage("The team description must contain no more than 200 characters");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Name);
    }

    [Fact]
    public async Task Validation_TeamWithTheSameNameAlreadyExists_ShouldHaveErrors()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var teams = TestData.TeamsFaker
            .RuleFor(t => t.TenantId, _ => tenant.Id)
            .Generate(2);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Teams.AddRangeAsync(teams);
        await _dbContextMock.SaveChangesAsync();

        var updateTeamRequest = TestData.UpdateTeamFaker
            .RuleFor(u => u.Name, teams[1].Name)
            .Generate();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.RouteValues.Add("id", teams[0].Id);
        _httpContextAccessorMock
            .SetupGet(c => c.HttpContext)
            .Returns(httpContext);

        //Act
        var actualResult = await _updateTeamRequestValidator.TestValidateAsync(updateTeamRequest);

        //Assert
        actualResult.ShouldHaveValidationErrorFor(r => r.Name)
            .WithErrorMessage("Team with the same name already exists");
        actualResult.ShouldNotHaveValidationErrorFor(r => r.Description);
    }

    private static class TestData
    {
        public static Faker<Tenant> TenantFaker =>
            new Faker<Tenant>()
                .RuleFor(t => t.Id, f => Guid.NewGuid())
                .RuleFor(t => t.Name, f => f.Company.CompanyName())
                .RuleFor(t => t.CreatedDateTimeUtc, f => DateTime.UtcNow);

        public static Faker<Team> TeamsFaker =>
            new Faker<Team>()
                .RuleFor(u => u.Name, f => f.Company.CompanyName())
                .RuleFor(u => u.Description, f => f.Name.JobDescriptor().ClampLength(1, 30));

        public static Faker<UpdateTeamRequest> UpdateTeamFaker =>
            new Faker<UpdateTeamRequest>()
                .RuleFor(u => u.Name, f => f.Company.CompanyName())
                .RuleFor(u => u.Description, f => f.Lorem.Sentence(2));
    }
}
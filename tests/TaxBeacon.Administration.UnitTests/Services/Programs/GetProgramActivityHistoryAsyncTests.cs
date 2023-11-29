using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    [Fact]
    public async Task GetProgramActivityHistoryAsync_ProgramExistsInTenantAndUserIsSuperAdminAndNotInTenant_ReturnsListOfProgramActivityLogsInDescendingOrderByDate()
    {
        // Arrange
        var tenant = await _dbContextMock.Tenants.FirstOrDefaultAsync();
        var program = TestData.ProgramFaker.Generate();
        program.TenantsPrograms = new List<TenantProgram> { new() { TenantId = tenant!.Id } };
        var programActivities = TestData.ProgramActivityLogFaker
            .RuleFor(x => x.ProgramId, _ => program.Id)
            .RuleFor(x => x.TenantId, _ => null)
            .Generate(2);
        var tenantProgramActivities = TestData.ProgramActivityLogFaker
            .RuleFor(x => x.ProgramId, _ => program.Id)
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .Generate(3);

        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.ProgramActivityLogs.AddRangeAsync(programActivities);
        await _dbContextMock.ProgramActivityLogs.AddRangeAsync(tenantProgramActivities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.IsSuperAdmin).Returns(true);
        _currentUserServiceMock.Setup(x => x.IsUserInTenant).Returns(false);

        // Act
        var actualResult = await _programService
            .GetProgramActivityHistoryAsync(program.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(2);
            activitiesResult.Query.Should().BeInDescendingOrder(x => x.Date);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task GetProgramActivityHistoryAsync_ProgramExistsAndUserInTenant_ReturnsListOfTenantProgramActivityLogsInDescendingOrderByDate(
        bool isSuperAdmin, bool isUserInTenant)
    {
        // Arrange
        var tenant = await _dbContextMock.Tenants.FirstOrDefaultAsync();
        var program = TestData.ProgramFaker.Generate();
        program.TenantsPrograms = new List<TenantProgram> { new() { TenantId = tenant!.Id } };
        var programActivities = TestData.ProgramActivityLogFaker
            .RuleFor(x => x.ProgramId, _ => program.Id)
            .RuleFor(x => x.TenantId, _ => null)
            .Generate(2);
        var tenantProgramActivities = TestData.ProgramActivityLogFaker
            .RuleFor(x => x.ProgramId, _ => program.Id)
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .Generate(3);

        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.ProgramActivityLogs.AddRangeAsync(programActivities);
        await _dbContextMock.ProgramActivityLogs.AddRangeAsync(tenantProgramActivities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.IsSuperAdmin).Returns(isSuperAdmin);
        _currentUserServiceMock.Setup(x => x.IsUserInTenant).Returns(isUserInTenant);

        // Act
        var actualResult = await _programService
            .GetProgramActivityHistoryAsync(program.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var activitiesResult, out _).Should().BeTrue();
            activitiesResult.Count.Should().Be(1);
            activitiesResult.Query.Count().Should().Be(3);
            activitiesResult.Query.Should().BeInDescendingOrder(x => x.Date);
        }
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task GetProgramActivityHistoryAsync_ProgramDoesNotExistInTenantAndUserInTenantId_ReturnsNotFound(
        bool isSuperAdmin, bool isUserInTenant)
    {
        // Arrange
        var program = TestData.ProgramFaker.Generate();

        await _dbContextMock.Programs.AddAsync(program);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.IsSuperAdmin).Returns(isSuperAdmin);
        _currentUserServiceMock.Setup(x => x.IsUserInTenant).Returns(isUserInTenant);

        // Act
        var actualResult = await _programService.GetProgramActivityHistoryAsync(program.Id);

        //Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }
}

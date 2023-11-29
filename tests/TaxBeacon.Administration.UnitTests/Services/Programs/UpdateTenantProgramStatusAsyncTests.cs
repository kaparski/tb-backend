using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Administration.UnitTests.Services.Programs;
public partial class ProgramServiceTests
{
    [Fact]
    public async Task UpdateTenantProgramStatusAsync_ActiveProgramStatusAndProgramId_UpdatedProgram()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var tenantProgram = TestData.TenantProgramFaker.Generate();
        var currentDate = DateTime.UtcNow;

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TenantsPrograms.AddAsync(tenantProgram);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock
            .Setup(s => s.TenantRoles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(s => s.Roles)
            .Returns(Array.Empty<string>());

        //Act
        var actualResult = await _programService.UpdateTenantProgramStatusAsync(tenantProgram.ProgramId, Status.Active);

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var programDetails, out _).Should().BeTrue();
            programDetails.Status.Should().Be(Status.Active);
            programDetails.DeactivationDateTimeUtc.Should().BeNull();
            programDetails.ReactivationDateTimeUtc.Should().Be(currentDate);
        }
    }

    [Fact]
    public async Task UpdateTenantProgramStatusAsync_DeactivatedProgramStatusAndProgramId_UpdatedTenantProgram()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var tenantProgram = TestData.TenantProgramFaker.Generate();
        var currentDate = DateTime.UtcNow;

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TenantsPrograms.AddAsync(tenantProgram);
        await _dbContextMock.SaveChangesAsync();

        _dateTimeServiceMock
            .Setup(ds => ds.UtcNow)
            .Returns(currentDate);

        _currentUserServiceMock
            .Setup(s => s.TenantRoles)
            .Returns(Array.Empty<string>());

        _currentUserServiceMock
            .Setup(s => s.Roles)
            .Returns(Array.Empty<string>());

        //Act
        var actualResult =
            await _programService.UpdateTenantProgramStatusAsync(tenantProgram.ProgramId, Status.Deactivated);

        //Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var programDetails, out _).Should().BeTrue();
            programDetails.Status.Should().Be(Status.Deactivated);
            programDetails.ReactivationDateTimeUtc.Should().BeNull();
            programDetails.DeactivationDateTimeUtc.Should().Be(currentDate);
        }
    }

    [Theory]
    [MemberData(nameof(TestData.UpdatedStatusInvalidData), MemberType = typeof(TestData))]
    public async Task UpdateTenantProgramStatusAsync_ProgramStatusAndProgramIdNotInDb_ReturnNotFound(Status status,
        Guid programId)
    {
        //Act
        var actualResult = await _programService.UpdateTenantProgramStatusAsync(programId, status);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out _, out _).Should().BeTrue();
        }
    }
}

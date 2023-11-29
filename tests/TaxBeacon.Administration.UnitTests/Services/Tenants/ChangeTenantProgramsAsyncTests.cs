using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.Tenants;
public partial class TenantServiceTests
{
    [Fact]
    public async Task ChangeTenantProgramsAsync_TenantWithoutProgramsAndNewProgramsIds_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker
            .RuleFor(t => t.TenantsPrograms, _ => Enumerable.Empty<TenantProgram>().ToList())
            .Generate();
        var programs = TestData.ProgramFaker.Generate(3);
        var programsIds = programs.Select(p => p.Id).ToList();

        await _dbContextMock.Tenants.AddRangeAsync(tenant);
        await _dbContextMock.Programs.AddRangeAsync(programs);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .Setup(s => s.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _tenantService.ChangeTenantProgramsAsync(tenant.Id, programsIds);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out _, out _).Should().BeTrue();

            var actualTenantProgramsId = await _dbContextMock.TenantsPrograms
                .Where(tp => tp.TenantId == tenant.Id && tp.IsDeleted == false)
                .Select(tp => tp.ProgramId)
                .ToListAsync();
            actualTenantProgramsId.Count.Should().Be(3);
            actualTenantProgramsId.Should().Contain(programsIds);

            var actualActivityLog = await _dbContextMock.TenantActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(TenantEventType.TenantAssignProgramsEvent);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(2));
        }
    }

    [Fact]
    public async Task ChangeTenantProgramsAsync_TenantWithDisabledProgramsAndReactivateProgramsIds_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        foreach (var tenantTenantsProgram in tenant.TenantsPrograms)
        {
            tenantTenantsProgram.IsDeleted = true;
            tenantTenantsProgram.DeletedDateTimeUtc = DateTime.UtcNow;
        }

        await _dbContextMock.Tenants.AddRangeAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var reactivateProgramIds = new[] { tenant.TenantsPrograms.First().ProgramId };
        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .Setup(s => s.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _tenantService.ChangeTenantProgramsAsync(tenant.Id, reactivateProgramIds);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out _, out _).Should().BeTrue();

            var actualTenantProgramsId = await _dbContextMock.TenantsPrograms
                .Where(tp => tp.TenantId == tenant.Id && tp.IsDeleted == false)
                .Select(tp => tp.ProgramId)
                .ToListAsync();
            actualTenantProgramsId.Count.Should().Be(1);
            actualTenantProgramsId.Should().BeEquivalentTo(reactivateProgramIds);

            var actualActivityLog = await _dbContextMock.TenantActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(TenantEventType.TenantAssignProgramsEvent);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(2));
        }
    }

    [Fact]
    public async Task ChangeTenantProgramsAsync_TenantWithProgramsAndNewProgramsIds_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();

        await _dbContextMock.Tenants.AddRangeAsync(tenant);
        await _dbContextMock.SaveChangesAsync();

        var programsIds = new[] { tenant.TenantsPrograms.First().ProgramId };
        var currentDate = DateTime.UtcNow;

        _dateTimeServiceMock
            .SetupSequence(s => s.UtcNow)
            .Returns(currentDate);

        // Act
        var actualResult = await _tenantService.ChangeTenantProgramsAsync(tenant.Id, programsIds);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out _, out _).Should().BeTrue();

            var actualTenantProgramsId = await _dbContextMock.TenantsPrograms
                .Where(tp => tp.TenantId == tenant.Id && tp.IsDeleted == false)
                .Select(tp => tp.ProgramId)
                .ToListAsync();
            actualTenantProgramsId.Count.Should().Be(1);
            actualTenantProgramsId.Should().Contain(programsIds);

            var actualActivityLog = await _dbContextMock.TenantActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(TenantEventType.TenantUnassignProgramEvent);
            actualActivityLog?.TenantId.Should().Be(tenant.Id);

            _dateTimeServiceMock
                .Verify(ds => ds.UtcNow, Times.Exactly(2));
        }
    }

    [Fact]
    public async Task ChangeTenantProgramsAsync_NotExistingTenantId_ReturnsNotFound()
    {
        // Act
        var actualResult = await _tenantService.ChangeTenantProgramsAsync(Guid.NewGuid(), Array.Empty<Guid>());

        // Assert
        actualResult.IsT0.Should().BeFalse();
        actualResult.IsT1.Should().BeTrue();
    }
}

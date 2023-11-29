using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums.Administration.Activities;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.UnitTests.Entities;
public partial class EntityServiceTests
{
    [Fact]
    public async Task UpdateStateId_StateIdExistsInDb_ReturnsStateId()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .Generate();

        var stateId = TestData.StateIdFaker
            .RuleFor(s => s.TenantId, _ => tenant.Id)
            .RuleFor(s => s.EntityId, _ => entity.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.StateIds.AddAsync(stateId);
        await _dbContextMock.SaveChangesAsync();

        var currentDate = DateTime.UtcNow;

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        _dateTimeServiceMock
            .SetupGet(s => s.UtcNow)
            .Returns(currentDate);

        var updateStateIdDto = TestData.AddStateIdFaker
            .RuleFor(s => s.StateIdType, f =>
            {
                var availableTypes = StateIdType.List.Except(new[] { StateIdType.FromName(stateId.StateIdType) });

                return f.PickRandom(availableTypes);
            })
            .Generate()
            .Adapt<UpdateStateIdDto>();

        var expectedStateId = updateStateIdDto.Adapt<StateId>();
        expectedStateId.Id = stateId.Id;
        expectedStateId.EntityId = stateId.EntityId;
        expectedStateId.TenantId = stateId.TenantId;

        // Act
        var actualResult = await _entityService.UpdateStateIdAsync(entity.Id,
            stateId.Id,
            updateStateIdDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            actualResult.TryPickT0(out var actualStateId, out _).Should().BeTrue();
            actualStateId.Should().BeEquivalentTo(expectedStateId, opt => opt.ExcludingMissingMembers());

            var actualActivityLog = await _dbContextMock.EntityActivityLogs.LastOrDefaultAsync();
            actualActivityLog.Should().NotBeNull();
            actualActivityLog?.Date.Should().Be(currentDate);
            actualActivityLog?.EventType.Should().Be(EntityEventType.EntityStateIdUpdated);
            actualActivityLog?.EntityId.Should().Be(entity.Id);

            _dateTimeServiceMock.VerifyGet(s => s.UtcNow, Times.Once);
        }
    }

    [Fact]
    public async Task UpdateStateId_StateIdDoesNotExistInDb_ReturnsNotFound()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var updateStateIdDto = TestData.AddStateIdFaker
            .Generate()
            .Adapt<UpdateStateIdDto>();

        // Act
        var actualResult = await _entityService.UpdateStateIdAsync(entity.Id,
            Guid.NewGuid(),
            updateStateIdDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task UpdateStateId_StateIdDoesNotBelongToEntity_ReturnsNotFound()
    {
        // Act
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .Generate(2);

        var stateId = TestData.StateIdFaker
            .RuleFor(s => s.TenantId, _ => tenant.Id)
            .RuleFor(s => s.EntityId, _ => entities[0].Id)
            .Generate();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.StateIds.AddRangeAsync(stateId);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        var updateStateIdDto = TestData.AddStateIdFaker
            .Generate()
            .Adapt<UpdateStateIdDto>();

        // Act
        var actualResult = await _entityService.UpdateStateIdAsync(entities[^1].Id,
            stateId.Id,
            updateStateIdDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }

    [Fact]
    public async Task UpdateStateId_StateIdAndEntityFromAnotherTenant_ReturnsNotFound()
    {
        // Act
        var tenants = TestData.TenantFaker.Generate(2);
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenants[0].Id)
            .Generate();
        var entity = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenants[0].Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .Generate();

        var stateId = TestData.StateIdFaker
            .RuleFor(s => s.TenantId, _ => tenants[0].Id)
            .RuleFor(s => s.EntityId, _ => entity.Id)
            .Generate();

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.StateIds.AddRangeAsync(stateId);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[^1].Id);

        var updateStateIdDto = TestData.AddStateIdFaker
            .Generate()
            .Adapt<UpdateStateIdDto>();

        // Act
        var actualResult = await _entityService.UpdateStateIdAsync(entity.Id,
            stateId.Id,
            updateStateIdDto);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT0.Should().BeFalse();
            actualResult.IsT1.Should().BeTrue();
        }
    }
}

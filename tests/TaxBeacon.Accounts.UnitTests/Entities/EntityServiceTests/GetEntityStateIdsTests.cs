using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.Accounts.Entities.Models;

namespace TaxBeacon.Accounts.UnitTests.Entities;
public partial class EntityServiceTests
{
    [Fact]
    public async Task GetEntityStateIds_ReturnsStateIds()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .Generate();

        var entities = TestData.EntityFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.AccountId, _ => account.Id)
            .Generate(3);

        var stateIds = TestData.StateIdFaker
            .RuleFor(x => x.TenantId, _ => tenant.Id)
            .RuleFor(x => x.Entity, f => f.PickRandom(entities))
            .Generate(8);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.StateIds.AddRangeAsync(stateIds);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        var expectedStateIds = entities[1].StateIds;

        // Act
        var actualResult = _entityService.GetEntityStateIdsAsync(entities[1].Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveCount(expectedStateIds.Count)
                .And.AllBeOfType<StateIdDto>()
                .And.BeEquivalentTo(actualResult, opt => opt.ExcludingMissingMembers());
        }
    }
}

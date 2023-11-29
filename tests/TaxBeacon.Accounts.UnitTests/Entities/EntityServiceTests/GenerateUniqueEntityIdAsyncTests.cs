using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Entities;
public partial class EntityServiceTests
{
    [Fact]
    public async Task GenerateUniqueEntityIdAsync_EntityDoesNotExistInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var idIncrement = 1_000_000;
        var entity = TestData.EntityFaker
            .RuleFor(e => e.TenantId, _ => tenant.Id)
            .RuleFor(e => e.AccountId, _ => account.Id)
            .RuleFor(e => e.EntityId, () => "E" + idIncrement++.ToString())
            .Generate(10000);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);
        var entityIds = entity.Select(x => x.EntityId).ToHashSet();
        for (var i = 0; i < 100; i++)
        {
            var actualResult = await _entityService.GenerateUniqueEntityIdAsync(CancellationToken.None);

            actualResult.IsT0.Should().BeTrue();
            entityIds.Should().NotContain(actualResult.AsT0);
        }
    }
}

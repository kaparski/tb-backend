using FluentAssertions.Execution;
using FluentAssertions;
using OneOf.Types;
using TaxBeacon.Accounts.Entities.Models;

namespace TaxBeacon.Accounts.UnitTests.Entities;
public partial class EntityServiceTests
{
    [Fact]
    public async Task QueryEntities_AccountExists_ReturnsEntities()
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

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        var expectedEntities = entities
            .Where(a => a.TenantId == tenant.Id)
            .ToList();

        // Act
        var actualResult = _entityService.QueryEntitiesAsync(account.Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var actualEntities, out _).Should().BeTrue();
            actualEntities.Should().HaveCount(expectedEntities.Count)
                .And.AllBeOfType<EntityDto>()
                .And.BeEquivalentTo(expectedEntities, opt => opt.ExcludingMissingMembers());
        }
    }

    [Fact]
    public async Task QueryEntities_AccountDoesNotExist_ReturnsEntities()
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

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var oneOf = _entityService.QueryEntitiesAsync(new Guid());

        // Assert
        using (new AssertionScope())
        {
            oneOf.IsT1.Should().BeTrue();
            var result = oneOf.AsT1;
            result.Should().BeOfType<NotFound>();
        }
    }
}

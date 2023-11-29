using FluentAssertions.Execution;
using FluentAssertions;

namespace TaxBeacon.Accounts.UnitTests.Entities;
public partial class EntityServiceTests
{
    [Fact]
    public async Task GetEntityDetailsAsync_EntityExists_ReturnsAccountDetailsDto()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var entities = TestData.EntityFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .RuleFor(a => a.AccountId, _ => account.Id)
            .RuleFor(a => a.Phones, _ => TestData.PhoneFaker.Generate(3))
            .Generate(3);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(tenant.Id);

        // Act
        var actualResult = await _entityService.GetEntityDetailsAsync(entities[0].Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var actualEntity, out _).Should().BeTrue();
            actualEntity.Should().BeEquivalentTo(entities[0], opt => opt.ExcludingMissingMembers());
            actualEntity.Phones.Should().BeEquivalentTo(entities[0].Phones, opt => opt.ExcludingMissingMembers());
        }
    }

    [Fact]
    public async Task GetEntityDetailsAsync_NonExistingTenantId_ReturnsNotFound()
    {
        // Act
        var actualResult = await _entityService.GetEntityDetailsAsync(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }

    [Fact]
    public async Task GetEntityDetailsAsync_EntityTenantIdNotEqualCurrentUserTenantId_ReturnsNotFound()
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var entities = TestData.EntityFaker
            .RuleFor(a => a.TenantId, _ => tenants[^1].Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Entities.AddRangeAsync(entities);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[0].Id);

        // Act
        var actualResult = await _entityService.GetEntityDetailsAsync(entities[0].Id);

        // Assert
        using (new AssertionScope())
        {
            actualResult.IsT1.Should().BeTrue();
            actualResult.IsT0.Should().BeFalse();
        }
    }
}

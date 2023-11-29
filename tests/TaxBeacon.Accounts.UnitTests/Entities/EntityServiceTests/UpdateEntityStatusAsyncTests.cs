using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Accounts.UnitTests.Entities;
public partial class EntityServiceTests
{
    [Theory]
    [InlineData(Status.Active)]
    [InlineData(Status.Deactivated)]
    public async Task UpdateEntityStatusAsync_EntityExists_UpdatedEntity(Status status)
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var entity = TestData.EntityFaker
            .RuleFor(x => x.TenantId, tenant.Id)
            .Generate();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var actualResult = await _entityService.UpdateEntityStatusAsync(entity.Id, status);

        //Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var updatedEntity, out _).Should().BeTrue();
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            updatedEntity.Status.Should().Be(status);
        }
    }

    [Fact]
    public async Task UpdateEntityStatusAsync_EntityDoesNotExist_NotFound()
    {
        //Arrange
        var tenant = TestData.TenantFaker.Generate();
        var entity = TestData.EntityFaker
            .RuleFor(x => x.TenantId, tenant.Id)
            .Generate();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Entities.AddAsync(entity);
        await _dbContextMock.SaveChangesAsync();

        //Act
        var actualResult = await _entityService.UpdateEntityStatusAsync(new Guid(), Status.Active);

        //Assert
        actualResult.IsT1.Should().BeTrue();
    }
}


using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.EntityFrameworkCore;
using Moq;
using TaxBeacon.Accounts.Entities.Models;

namespace TaxBeacon.Accounts.UnitTests.Entities;
public partial class EntityServiceTests
{
    [Fact]
    public async Task ImportAccountEntitiesAsync_ValidInputData_ReturnsSuccess()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var streamMock = new Mock<Stream>();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        var entities = TestData.ImportEntityModelFaker.Generate(5);

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        _csvServiceMock
            .Setup(c => c.Read<ImportEntityModel>(streamMock.Object))
            .Returns(entities);

        // Act
        var result = await _entityService.ImportAccountEntitiesAsync(account.Id, streamMock.Object, default);

        // Assert
        using (new AssertionScope())
        {
            var importedEntitiesCount =
                await _dbContextMock.Entities.Where(e => e.AccountId == account.Id).CountAsync();
            importedEntitiesCount.Should().Be(entities.Count);
            result.IsT0.Should().BeTrue();
            result.IsT1.Should().BeFalse();
        }
    }

    [Fact]
    public async Task ImportAccountEntitiesAsync_InvalidInputData_ReturnsInvalidOperation()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var account = TestData.AccountFaker
            .RuleFor(a => a.TenantId, _ => tenant.Id)
            .Generate();
        var streamMock = new Mock<Stream>();

        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Accounts.AddAsync(account);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(service => service.TenantId)
            .Returns(tenant.Id);

        var entities = TestData.ImportEntityModelFaker
            .RuleFor(x => x.Name, _ => null)
            .Generate(5);

        _csvServiceMock
            .Setup(c => c.Read<ImportEntityModel>(streamMock.Object))
            .Returns(entities);

        // Act
        var result = await _entityService.ImportAccountEntitiesAsync(account.Id, streamMock.Object, default);

        // Assert
        result.IsT0.Should().BeFalse();
        result.IsT1.Should().BeTrue();
    }
}

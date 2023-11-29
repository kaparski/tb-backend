using FluentAssertions.Execution;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace TaxBeacon.Administration.UnitTests.Services.TableFilters;
public partial class TableFiltersServiceTests
{
    [Fact]
    public async Task DeleteFiltersAsync_FilterExistsInDb_ReturnsEmptyList()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var tableFilter = TestData.TableFilterFaker.Generate();
        tableFilter.UserId = user.Id;
        tableFilter.TenantId = tenant.Id;

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TableFilters.AddAsync(tableFilter);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        // Act
        var deleteResultOneOf = await _tableFiltersService.DeleteFilterAsync(tableFilter.Id);

        // Assert
        using (new AssertionScope())
        {
            deleteResultOneOf.IsT0.Should().BeTrue();
            deleteResultOneOf.IsT1.Should().BeFalse();
            deleteResultOneOf.TryPickT0(out var tableFilters, out _);
            tableFilters.Should().BeEmpty();

            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            (await _dbContextMock.TableFilters.AnyAsync(tf => tf.Id == tableFilter.Id)).Should().BeFalse();
        }
    }

    [Fact]
    public async Task DeleteFiltersAsync_FilterExistsInDbAndUserWithoutTenant_ReturnsEmptyList()
    {
        // Arrange
        var user = TestData.UserFaker.Generate();
        var tableFilter = TestData.TableFilterFaker.Generate();
        tableFilter.UserId = user.Id;

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.TableFilters.AddAsync(tableFilter);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(Guid.Empty);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        // Act
        var deleteResultOneOf = await _tableFiltersService.DeleteFilterAsync(tableFilter.Id);

        // Assert
        using (new AssertionScope())
        {
            deleteResultOneOf.IsT0.Should().BeTrue();
            deleteResultOneOf.IsT1.Should().BeFalse();
            deleteResultOneOf.TryPickT0(out var tableFilters, out _);
            tableFilters.Should().BeEmpty();

            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);
            (await _dbContextMock.TableFilters.AnyAsync(tf => tf.Id == tableFilter.Id)).Should().BeFalse();
        }
    }

    [Fact]
    public async Task DeleteFiltersAsync_FilterNotExistsInDb_ReturnsNotFound()
    {
        // Arrange
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var tableFilter = TestData.TableFilterFaker.Generate();
        tableFilter.UserId = user.Id;
        tableFilter.TenantId = tenant.Id;

        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.TableFilters.AddAsync(tableFilter);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        // Act
        var deleteResultOneOf = await _tableFiltersService.DeleteFilterAsync(Guid.NewGuid());

        // Assert
        using (new AssertionScope())
        {
            deleteResultOneOf.IsT0.Should().BeFalse();
            deleteResultOneOf.IsT1.Should().BeTrue();
        }
    }
}

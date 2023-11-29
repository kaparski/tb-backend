using FluentAssertions.Execution;
using FluentAssertions;
using Mapster;
using Microsoft.EntityFrameworkCore;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.Administration.UnitTests.Services.TableFilters;
public partial class TableFiltersServiceTests
{
    [Fact]
    public async Task CreateFilterAsync_ValidCreateTableFilterDto_ReturnsCollectionOfTableFilters()
    {
        // Arrange
        var createTableFilterDto = TestData.CreateTableFilterDtoFaker.Generate();
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        await _dbContextMock.Tenants.AddAsync(tenant);
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenant.Id);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        // Act
        var createdTableFilterOneOf = await _tableFiltersService.CreateFilterAsync(createTableFilterDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);

            createdTableFilterOneOf.IsT0.Should().BeTrue();
            createdTableFilterOneOf.IsT1.Should().BeFalse();
            createdTableFilterOneOf.TryPickT0(out var tableFilters, out _);
            tableFilters.Should().NotBeNullOrEmpty();
            tableFilters.Should().Contain(tf =>
                tf.Name.Equals(createTableFilterDto.Name, StringComparison.OrdinalIgnoreCase));

            var actualDbTableFilter = await _dbContextMock.TableFilters.LastOrDefaultAsync();
            actualDbTableFilter.Should().NotBeNull();
            actualDbTableFilter?.TableType.Should().Be(createTableFilterDto.TableType);
            actualDbTableFilter?.Name.Should().Be(createTableFilterDto.Name);
            actualDbTableFilter?.Configuration.Should().Be(createTableFilterDto.Configuration);
            actualDbTableFilter?.UserId.Should().Be(user.Id);
            actualDbTableFilter?.TenantId.Should().Be(tenant.Id);
        }
    }

    [Fact]
    public async Task CreateFilterAsync_ValidCreateTableFilterDtoAndUserWithoutTenant_ReturnsCollectionOfTableFilters()
    {
        // Arrange
        var createTableFilterDto = TestData.CreateTableFilterDtoFaker.Generate();
        var user = TestData.UserFaker.Generate();
        await _dbContextMock.Users.AddAsync(user);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(Guid.Empty);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(user.Id);

        // Act
        var createdTableFilterOneOf = await _tableFiltersService.CreateFilterAsync(createTableFilterDto);

        // Assert
        using (new AssertionScope())
        {
            (await _dbContextMock.SaveChangesAsync()).Should().Be(0);

            createdTableFilterOneOf.IsT0.Should().BeTrue();
            createdTableFilterOneOf.IsT1.Should().BeFalse();
            createdTableFilterOneOf.TryPickT0(out var tableFilters, out _);
            tableFilters.Should().NotBeNullOrEmpty();
            tableFilters.Should().Contain(tf =>
                tf.Name.Equals(createTableFilterDto.Name, StringComparison.OrdinalIgnoreCase));

            var actualDbTableFilter = await _dbContextMock.TableFilters.LastOrDefaultAsync();
            actualDbTableFilter.Should().NotBeNull();
            actualDbTableFilter?.TableType.Should().Be(createTableFilterDto.TableType);
            actualDbTableFilter?.Name.Should().Be(createTableFilterDto.Name);
            actualDbTableFilter?.Configuration.Should().Be(createTableFilterDto.Configuration);
            actualDbTableFilter?.UserId.Should().Be(user.Id);
            actualDbTableFilter?.TenantId.Should().BeNull();
        }
    }

    [Fact]
    public async Task CreateFilterAsync_ValidCreateTableFilterDtoWithExistingFilterName_ReturnsNameAlreadyExists()
    {
        // Arrange
        var createTableFilterDto = TestData.CreateTableFilterDtoFaker.Generate();
        var tenant = TestData.TenantFaker.Generate();
        var user = TestData.UserFaker.Generate();
        var tableFilter = createTableFilterDto.Adapt<TableFilter>();
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
        var createdTableFilterOneOf = await _tableFiltersService.CreateFilterAsync(createTableFilterDto);

        // Assert
        using (new AssertionScope())
        {
            createdTableFilterOneOf.IsT0.Should().BeFalse();
            createdTableFilterOneOf.IsT1.Should().BeTrue();
        }
    }
}

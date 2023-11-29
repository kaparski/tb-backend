using Bogus;
using FluentAssertions.Execution;
using TaxBeacon.Common.Enums;
using FluentAssertions;

namespace TaxBeacon.Administration.UnitTests.Services.TableFilters;
public partial class TableFiltersServiceTests
{
    [Theory]
    [InlineData(EntityType.User)]
    [InlineData(EntityType.Role)]
    [InlineData(EntityType.Tenant)]
    [InlineData(EntityType.Team)]
    [InlineData(EntityType.TenantDivision)]
    [InlineData(EntityType.Department)]
    [InlineData(EntityType.ServiceArea)]
    [InlineData(EntityType.JobTitle)]
    [InlineData(EntityType.Program)]
    [InlineData(EntityType.TenantProgram)]
    [InlineData(EntityType.Contact)]
    [InlineData(EntityType.Locations)]
    [InlineData(EntityType.Entity)]
    [InlineData(EntityType.ClientProspects)]
    [InlineData(EntityType.Client)]
    [InlineData(EntityType.ReferralProspects)]
    public async Task GetFiltersAsync_TableType_ReturnsCollectionOfTableFilters(EntityType tableType)
    {
        // Arrange
        var tenants = TestData.TenantFaker.Generate(2);
        var users = TestData.UserFaker.Generate(2);
        var firstUserFilters = TestData.TableFilterFaker
            .RuleFor(tf => tf.UserId, _ => users[0].Id)
            .RuleFor(tf => tf.TenantId, _ => tenants[0].Id)
            .Generate(5);

        var secondUserFilters = TestData.TableFilterFaker
            .RuleFor(tf => tf.UserId, _ => users[^1].Id)
            .RuleFor(tf => tf.TenantId, _ => tenants[^1].Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TableFilters.AddRangeAsync(firstUserFilters);
        await _dbContextMock.TableFilters.AddRangeAsync(secondUserFilters);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[0].Id);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(users[0].Id);

        var expectedFilters = firstUserFilters
            .Where(tf => tf.TableType == tableType)
            .ToList();

        // Act
        var actualResult = await _tableFiltersService.GetFiltersAsync(tableType);

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveSameCount(expectedFilters);
            actualResult.Select(dto => dto.Id).Should().Equal(expectedFilters.Select(tf => tf.Id));
        }
    }

    [Fact]
    public async Task GetFiltersAsync_UserWithoutFilters_ReturnsEmptyCollection()
    {
        // Arrange
        var tableType = new Faker().PickRandom<EntityType>();
        var tenants = TestData.TenantFaker.Generate(2);
        var users = TestData.UserFaker.Generate(2);
        var firstUserFilters = TestData.TableFilterFaker
            .RuleFor(tf => tf.UserId, _ => users[0].Id)
            .RuleFor(tf => tf.TenantId, _ => tenants[0].Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TableFilters.AddRangeAsync(firstUserFilters);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(tenants[^1].Id);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(users[^1].Id);

        // Act
        var actualResult = await _tableFiltersService.GetFiltersAsync(tableType);

        // Assert
        actualResult.Should().HaveCount(0);
    }

    [Fact]
    public async Task GetFiltersAsync_UserWithoutTenant_ReturnsCollectionOfTableFilters()
    {
        // Arrange
        var tableType = new Faker().PickRandom<EntityType>();
        var tenants = TestData.TenantFaker.Generate(2);
        var users = TestData.UserFaker.Generate(2);
        var firstUserFilters = TestData.TableFilterFaker
            .RuleFor(tf => tf.UserId, _ => users[0].Id)
            .RuleFor(tf => tf.TenantId, _ => null)
            .Generate(5);

        var secondUserFilters = TestData.TableFilterFaker
            .RuleFor(tf => tf.UserId, _ => users[^1].Id)
            .RuleFor(tf => tf.TenantId, _ => tenants[^1].Id)
            .Generate(5);

        await _dbContextMock.Tenants.AddRangeAsync(tenants);
        await _dbContextMock.Users.AddRangeAsync(users);
        await _dbContextMock.TableFilters.AddRangeAsync(firstUserFilters);
        await _dbContextMock.TableFilters.AddRangeAsync(secondUserFilters);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock
            .Setup(s => s.TenantId)
            .Returns(Guid.Empty);

        _currentUserServiceMock
            .Setup(s => s.UserId)
            .Returns(users[0].Id);

        var expectedFilters = firstUserFilters
            .Where(tf => tf.TableType == tableType)
            .ToList();

        // Act
        var actualResult = await _tableFiltersService.GetFiltersAsync(tableType);

        // Assert
        using (new AssertionScope())
        {
            actualResult.Should().HaveSameCount(expectedFilters);
            actualResult.Select(dto => dto.Id).Should().Equal(expectedFilters.Select(tf => tf.Id));
        }
    }
}

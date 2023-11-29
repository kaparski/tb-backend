using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.Common.Exceptions;

namespace TaxBeacon.Administration.UnitTests.Services.ServiceAreas;
public partial class ServiceAreaServiceTests
{
    [Fact]
    public async Task QueryUsersAsync_ServiceAreaDoesNotExists_ShouldThrow()
    {
        // Arrange
        TestData.TestServiceArea.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(1));
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var task = _serviceAreaService.QueryUsersAsync(new Guid());

        // Arrange
        task.Exception!.InnerException.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public async Task QueryUsersAsync_ServiceAreaExists_ShouldReturnUsersInDescendingOrderByEmail()
    {
        // Arrange
        TestData.TestServiceArea.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(3));
        var serviceArea = TestData.TestServiceArea.Generate();
        await _dbContextMock.ServiceAreas.AddAsync(serviceArea);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var query = await _serviceAreaService.QueryUsersAsync(serviceArea.Id);
        var listOfUsers = query
            .OrderByDescending(u => u.Email)
            .ToArray();

        // Arrange
        using (new AssertionScope())
        {
            listOfUsers.Length.Should().Be(3);
            listOfUsers.Select(x => x.Email).Should().BeInDescendingOrder();
        }
    }
}

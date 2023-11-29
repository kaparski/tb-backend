using FluentAssertions.Execution;
using FluentAssertions;
using TaxBeacon.Common.Exceptions;

namespace TaxBeacon.Administration.UnitTests.Services.JobTitles;
public partial class JobTitleServiceTests
{
    [Fact]
    public async Task QueryUsersAsync_JobTitleDoesNotExists_ShouldThrow()
    {
        // Arrange
        TestData.TestJobTitle.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(1));
        var title = TestData.TestJobTitle.Generate();
        await _dbContextMock.JobTitles.AddAsync(title);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var task = _serviceAreaService.QueryUsersAsync(new Guid());

        // Arrange
        task.Exception!.InnerException.Should().BeOfType<NotFoundException>();
    }

    [Fact]
    public async Task QueryUsersAsync_JobTitleExists_ShouldReturnUsersInDescendingOrderByEmail()
    {
        // Arrange
        TestData.TestJobTitle.RuleFor(x => x.Users, _ => TestData.TestUser.Generate(3));
        var title = TestData.TestJobTitle.Generate();
        await _dbContextMock.JobTitles.AddAsync(title);
        await _dbContextMock.SaveChangesAsync();

        _currentUserServiceMock.Setup(x => x.TenantId).Returns(TestData.TestTenantId);

        // Act
        var query = await _serviceAreaService.QueryUsersAsync(title.Id);
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

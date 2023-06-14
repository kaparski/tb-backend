using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Reflection;
using TaxBeacon.Accounts.Entities;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Entities;
using TaxBeacon.API.Controllers.Entities.Responses;
using TaxBeacon.API.Controllers.Tenants.Responses;

namespace TaxBeacon.API.UnitTests.Controllers.Entities;
public class EntitiesControllerTest
{
    private readonly Mock<IEntityService> _entityServiceMock;
    private readonly EntitiesController _controller;
    public EntitiesControllerTest()
    {
        _entityServiceMock = new();
        _controller = new EntitiesController(_entityServiceMock.Object);
    }

    [Fact]
    public async Task Get_ExistingAccountId_ReturnSuccessStatusCode()
    {
        // Arrange
        _entityServiceMock.Setup(p => p.QueryEntitiesAsync(It.IsAny<Guid>())).ReturnsAsync(
                new Success<IQueryable<EntityDto>>(Enumerable.Empty<EntityDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.Get(new Guid());

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<IQueryable<EntityResponse>>();
        }
    }

    [Fact]
    public async Task Get_NonExistingAccountId_ReturnNotFoundStatusCode()
    {
        // Arrange
        _entityServiceMock.Setup(p => p.QueryEntitiesAsync(It.IsAny<Guid>())).ReturnsAsync(
            new NotFound());

        // Act
        var actualResponse = await _controller.Get(new Guid());

        // Arrange
        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Task<IActionResult>>)_controller.Get).Method;
        var permissions = new object[]
        {
            Common.Permissions.Entities.Read
        };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }
}

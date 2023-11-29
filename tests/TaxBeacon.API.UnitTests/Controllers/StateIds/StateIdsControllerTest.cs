using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Reflection;
using TaxBeacon.Accounts.Entities;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.StateIds;
using TaxBeacon.API.Controllers.StateIds.Requests;
using TaxBeacon.API.Controllers.StateIds.Responses;

namespace TaxBeacon.API.UnitTests.Controllers.StateIds;

public class StateIdsControllerTest
{
    private readonly Mock<IEntityService> _entityServiceMock;
    private readonly StateIdsController _controller;

    public StateIdsControllerTest()
    {
        _entityServiceMock = new();
        _controller = new StateIdsController(_entityServiceMock.Object);
    }

    [Fact]
    public void Get_ExistingAccountId_ReturnSuccessStatusCode()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.GetEntityStateIdsAsync(It.IsAny<Guid>()))
            .Returns(new List<StateIdDto>().AsQueryable());

        // Act
        var actualResponse = _controller.GetStateIds(new Guid());

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<IQueryable<StateIdResponse>>();
        }
    }

    [Fact]
    public void GetStateIds_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, IActionResult>)
            _controller.GetStateIds).Method;
        var permissions = new object[] { Common.Permissions.Entities.Read };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    [Fact]
    public async Task RemoveStateIdAsync_ReturnSuccess()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.RemoveStateIdFromEntityAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.RemoveStateIdAsync(new Guid(), new Guid());

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NoContentResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<NoContentResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }
    }

    [Fact]
    public async Task RemoveStateIdAsync_StateIdNotExistsInDb_ReturnsNotFound()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.RemoveStateIdFromEntityAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.RemoveStateIdAsync(new Guid(), new Guid());

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
    public void RemoveStateIdAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        var methodInfo =
            ((Func<Guid, Guid, CancellationToken, Task<IActionResult>>)_controller.RemoveStateIdAsync)
            .Method;

        var permissions = new object[] { Common.Permissions.Entities.ReadWrite };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    [Fact]
    public async Task AddStateIdsAsync_ValidData_ReturnSuccessStatusCode()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.AddStateIdsAsync(It.IsAny<Guid>(), It.IsAny<List<AddStateIdDto>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StateIdDto>());

        // Act
        var actualResponse =
            await _controller.AddStateIdsAsync(new Guid(), new AddStateIdsRequest());

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<IEnumerable<StateIdResponse>>();
        }
    }

    [Fact]
    public async Task AddStateIdsAsync_EntityNotExistsInDb_ReturnsNotFoundResponse()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.AddStateIdsAsync(It.IsAny<Guid>(), It.IsAny<List<AddStateIdDto>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse =
            await _controller.AddStateIdsAsync(new Guid(), new AddStateIdsRequest());

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<NotFoundResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void AddStateIdsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, AddStateIdsRequest, CancellationToken, Task<IActionResult>>)
            _controller.AddStateIdsAsync).Method;
        var permissions = new object[] { Common.Permissions.Entities.ReadWrite };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }

    [Fact]
    public async Task UpdatedStateIdAsync_ValidData_ReturnSuccessStatusCode()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.UpdateStateIdAsync(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<UpdateStateIdDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StateIdDto());

        // Act
        var actualResponse = await _controller
            .UpdateStateIdAsync(new Guid(), new Guid(), new UpdateStateIdRequest());

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<StateIdResponse>();
        }
    }

    [Fact]
    public async Task UpdateStateIdAsync_StateIdNotExistsInDb_ReturnsNotFoundResponse()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.UpdateStateIdAsync(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<UpdateStateIdDto>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller
            .UpdateStateIdAsync(new Guid(), new Guid(), new UpdateStateIdRequest());

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<NotFoundResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void UpdatedStateIdAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Guid, UpdateStateIdRequest, CancellationToken, Task<IActionResult>>)
            _controller.UpdateStateIdAsync).Method;
        var permissions = new object[] { Common.Permissions.Entities.ReadWrite };

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should()
                .Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }
}

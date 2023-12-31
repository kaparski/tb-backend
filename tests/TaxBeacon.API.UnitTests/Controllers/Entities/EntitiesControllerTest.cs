﻿using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf;
using OneOf.Types;
using System.Reflection;
using TaxBeacon.Accounts.Entities;
using TaxBeacon.Accounts.Entities.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Entities;
using TaxBeacon.API.Controllers.Entities.Responses;

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
    public void Get_ExistingAccountId_ReturnSuccessStatusCode()
    {
        // Arrange
        _entityServiceMock.Setup(p => p.QueryEntitiesAsync(It.IsAny<Guid>())).Returns(
                OneOf<IQueryable<EntityDto>, NotFound>.FromT0(new List<EntityDto>().AsQueryable()));

        // Act
        var actualResponse = _controller.Get(new Guid());

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
    public void Get_NonExistingAccountId_ReturnNotFoundStatusCode()
    {
        // Arrange
        _entityServiceMock.Setup(p => p.QueryEntitiesAsync(It.IsAny<Guid>())).Returns(
            new NotFound());

        // Act
        var actualResponse = _controller.Get(new Guid());

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
        var methodInfo = ((Func<Guid, IActionResult>)_controller.Get).Method;
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

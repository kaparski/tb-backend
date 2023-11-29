using FluentAssertions;
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
using TaxBeacon.API.Controllers.Entities.Requests;
using TaxBeacon.API.Controllers.Entities.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;

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
        _entityServiceMock
            .Setup(p => p.QueryEntitiesAsync(It.IsAny<Guid>()))
            .Returns(OneOf<IQueryable<EntityDto>, NotFound>.FromT0(new List<EntityDto>().AsQueryable()));

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
        _entityServiceMock
            .Setup(p => p.QueryEntitiesAsync(It.IsAny<Guid>()))
            .Returns(new NotFound());

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
            Common.Permissions.Entities.Read,
            Common.Permissions.Entities.ReadActivation,
            Common.Permissions.Entities.ReadWrite,
            Common.Permissions.Entities.ReadExport
        };

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
    public async Task GetEntityDetails_EntityExists_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        _entityServiceMock
            .Setup(x => x.GetEntityDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new EntityDetailsDto());

        // Act
        var actualResponse = await _controller.GetEntityDetails(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<EntityDetailsResponse>();
        }
    }

    [Fact]
    public async Task GetEntityDetails_EntityDoesNotExist_ReturnsNotFoundStatusCode()
    {
        // Arrange
        _entityServiceMock
            .Setup(x => x.GetEntityDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetEntityDetails(Guid.NewGuid(), default);

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
    public void GetEntityDetails_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.GetEntityDetails)
            .Method;

        var permissions = new object[]
        {
            Common.Permissions.Entities.Read, Common.Permissions.Entities.ReadWrite,
            Common.Permissions.Entities.ReadActivation
        };

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
    public async Task UpdateEntityStatusAsync_ExistingEntityId_ReturnSuccessStatusCode()
    {
        // Arrange
        _entityServiceMock.Setup(p =>
                p.UpdateEntityStatusAsync(It.IsAny<Guid>(), It.IsAny<Status>(), default))
            .ReturnsAsync(
                new EntityDetailsDto());

        // Act
        var actualResponse = await _controller.UpdateEntityStatusAsync(new Guid(), Status.Active, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<EntityDetailsResponse>();
        }
    }

    [Fact]
    public async Task UpdateEntityStatusAsync_NonExistingEntityId_ReturnNotFoundStatusCode()
    {
        // Arrange
        _entityServiceMock.Setup(p =>
                p.UpdateEntityStatusAsync(It.IsAny<Guid>(), It.IsAny<Status>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateEntityStatusAsync(new Guid(), Status.Active, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void UpdateEntityStatusAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, Status, CancellationToken, Task<IActionResult>>)_controller.UpdateEntityStatusAsync).Method;
        var permissions = new object[] { Common.Permissions.Entities.ReadActivation };

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

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportAccountEntitiesAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        await using var memStream = new MemoryStream();
        var resultFileType = fileType == FileType.Csv ? FileType.Zip : fileType;
        var request = new ExportAccountEntitiesRequest(fileType, "America/New_York");
        _entityServiceMock
            .Setup(x => x.ExportAccountEntitiesAsync(
                It.IsAny<Guid>(),
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FileStreamDto(memStream, $"entities.{resultFileType.ToString().ToLowerInvariant()}", resultFileType));

        // Act
        var actualResponse = await _controller.ExportAccountEntitiesAsync(Guid.NewGuid(), request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileStreamResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"entities.{resultFileType.ToString().ToLowerInvariant()}");
            actualResult.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "application/zip",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportAccountEntitiesAsync_AccountDoesNotExist_ReturnsNotFound(FileType fileType)
    {
        // Arrange
        var request = new ExportAccountEntitiesRequest(fileType, "America/New_York");
        _entityServiceMock
            .Setup(x => x.ExportAccountEntitiesAsync(
                It.IsAny<Guid>(),
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.ExportAccountEntitiesAsync(Guid.NewGuid(), request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as NotFoundResult;
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public void ExportAccountEntitiesAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, ExportAccountEntitiesRequest, CancellationToken, Task<IActionResult>>)
            _controller.ExportAccountEntitiesAsync).Method;
        var permissions = new object[] { Common.Permissions.Entities.ReadExport };

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
    public async Task CreateNewEntityAsync_NoConflictErrors_ReturnsEntityDetailsResponse()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.CreateNewEntityAsync(It.IsAny<Guid>(),
                It.IsAny<CreateEntityDto>(),
                default))
            .ReturnsAsync(new EntityDetailsDto());

        // Act
        var actualResponse = await _controller.CreateNewEntityAsync(new Guid(),
            new CreateEntityRequest { Country = Country.UnitedStates, Type = AccountEntityType.CCorp });

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<EntityDetailsResponse>();
        }
    }

    [Fact]
    public async Task CreateNewEntityAsync_AccountNotExists_ReturnsNotFoundResponse()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.CreateNewEntityAsync(It.IsAny<Guid>(),
                It.IsAny<CreateEntityDto>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.CreateNewEntityAsync(new Guid(),
            new CreateEntityRequest { Country = Country.UnitedStates, Type = AccountEntityType.CCorp });

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public async Task CreateNewEntityAsync_ConflictErrors_ReturnsConflictResponse()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.CreateNewEntityAsync(It.IsAny<Guid>(),
                It.IsAny<CreateEntityDto>(),
                default))
            .ReturnsAsync(new InvalidOperation("error"));

        // Act
        var actualResponse = await _controller.CreateNewEntityAsync(new Guid(),
            new CreateEntityRequest { Country = Country.UnitedStates, Type = AccountEntityType.CCorp });

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as ConflictObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status409Conflict);
            actualResult?.Value.Should().BeOfType<InvalidOperationResponse>();
        }
    }

    [Fact]
    public void CreateNewEntityAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, CreateEntityRequest, CancellationToken, Task<IActionResult>>)
            _controller.CreateNewEntityAsync).Method;
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
    public async Task UpdateEntityAsync_NoConflictErrors_ReturnsEntityDetailsResponse()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.UpdateEntityAsync(It.IsAny<Guid>(),
                It.IsAny<UpdateEntityDto>(),
                default))
            .ReturnsAsync(new EntityDetailsDto());

        // Act
        var actualResponse = await _controller.UpdateEntityAsync(new Guid(),
            new UpdateEntityRequest { Country = Country.UnitedStates, Type = AccountEntityType.CCorp },
            CancellationToken.None);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<EntityDetailsResponse>();
        }
    }

    [Fact]
    public async Task UpdateEntityAsync_AccountNotExists_ReturnsNotFoundResponse()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.UpdateEntityAsync(It.IsAny<Guid>(),
                It.IsAny<UpdateEntityDto>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateEntityAsync(new Guid(),
            new UpdateEntityRequest { Country = Country.UnitedStates, Type = AccountEntityType.CCorp },
            CancellationToken.None);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    [Fact]
    public async Task UpdateEntityAsync_ConflictErrors_ReturnsConflictResponse()
    {
        // Arrange
        _entityServiceMock
            .Setup(p => p.UpdateEntityAsync(It.IsAny<Guid>(),
                It.IsAny<UpdateEntityDto>(),
                default))
            .ReturnsAsync(new InvalidOperation("error"));

        // Act
        var actualResponse = await _controller.UpdateEntityAsync(new Guid(),
            new UpdateEntityRequest { Country = Country.UnitedStates, Type = AccountEntityType.CCorp },
            CancellationToken.None);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as ConflictObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status409Conflict);
            actualResult?.Value.Should().BeOfType<InvalidOperationResponse>();
        }
    }

    [Fact]
    public void UpdateEntityAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, UpdateEntityRequest, CancellationToken, Task<IActionResult>>)
            _controller.UpdateEntityAsync).Method;
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

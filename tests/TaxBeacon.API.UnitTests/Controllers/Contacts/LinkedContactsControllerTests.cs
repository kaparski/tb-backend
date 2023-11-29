using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf;
using OneOf.Types;
using System.Reflection;
using TaxBeacon.Accounts.Contacts;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Contacts;
using TaxBeacon.API.Controllers.Contacts.Requests;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.Common.Errors;

namespace TaxBeacon.API.UnitTests.Controllers.Contacts;

public class LinkedContactsControllerTests
{
    private readonly Mock<IContactService> _contactServiceMock;
    private readonly LinkedContactsController _controller;

    public LinkedContactsControllerTests()
    {
        _contactServiceMock = new();
        _controller = new LinkedContactsController(_contactServiceMock.Object);
    }

    [Fact]
    public async Task Get_ExistingContactId_ReturnSuccessStatusCode()
    {
        // Arrange
        _contactServiceMock.Setup(p => p
                .QueryLinkedContacts(It.IsAny<Guid>(), default))
            .ReturnsAsync(
                OneOf<IQueryable<LinkedContactDetailsDto>, NotFound>.FromT0(Enumerable.Empty<LinkedContactDetailsDto>()
                    .AsQueryable()));

        // Act
        var actualResponse = await _controller.Get(new Guid(), default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<IQueryable<LinkedContactDetailsResponse>>();
        }
    }

    [Fact]
    public async Task Get_NonExistingContactId_ReturnNotFoundStatusCode()
    {
        // Arrange
        _contactServiceMock.Setup(p => p
                .QueryLinkedContacts(It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.Get(new Guid(), default);

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
        var methodInfo = ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.Get)
            .Method;
        var permissions = new object[]
        {
            Common.Permissions.Contacts.Read, Common.Permissions.Contacts.ReadWrite,
            Common.Permissions.Contacts.ReadExport
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
    public async Task UnlinkContactFromContactAsync_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.UnlinkContactFromContactAsync(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.UnlinkContactFromContactAsync(Guid.NewGuid(),
            Guid.NewGuid(),
            default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkResult;
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }

    [Fact]
    public async Task UnlinkContactFromContactAsync_AccountOrContactNotExists_ReturnsNotFoundResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.UnlinkContactFromContactAsync(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UnlinkContactFromContactAsync(Guid.NewGuid(),
            Guid.NewGuid(),
            default);

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
    public void UnlinkContactFromContactAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, Guid, CancellationToken, Task<IActionResult>>)_controller.UnlinkContactFromContactAsync)
            .Method;
        var permissions = new object[] { Common.Permissions.Contacts.ReadWrite };

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
    public async Task LinkContactToContactAsync_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.LinkContactToContactAsync(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.LinkContactToContactAsync(Guid.NewGuid(),
            new LinkContactRequest(),
            default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkResult;
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }

    [Fact]
    public async Task LinkContactToContactAsync_ContactsNotExist_ReturnsNotFoundResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.LinkContactToContactAsync(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.LinkContactToContactAsync(Guid.NewGuid(),
            new LinkContactRequest(),
            default);

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
    public async Task LinkContactToContactAsync_ContactsAlreadyLinked_ReturnsConflictResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.LinkContactToContactAsync(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<string>(),
                default))
            .ReturnsAsync(new Conflict());

        // Act
        var actualResponse = await _controller.LinkContactToContactAsync(Guid.NewGuid(),
            new LinkContactRequest(),
            default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as ConflictResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        }
    }

    [Fact]
    public void LinkContactToContactAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, LinkContactRequest, CancellationToken, Task<IActionResult>>)_controller
                .LinkContactToContactAsync)
            .Method;
        var permissions = new object[] { Common.Permissions.Contacts.ReadWrite };

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

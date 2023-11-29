using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Reflection;
using TaxBeacon.Accounts.Contacts;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Contacts;
using TaxBeacon.API.Controllers.Contacts.Requests;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.API.Shared.Requests;
using TaxBeacon.API.Shared.Responses;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Models;

namespace TaxBeacon.API.UnitTests.Controllers.Contacts;

public partial class ContactsControllerTests
{
    private readonly Mock<IContactService> _contactServiceMock;
    private readonly ContactsController _controller;

    public ContactsControllerTests()
    {
        _contactServiceMock = new();
        _controller = new ContactsController(_contactServiceMock.Object);
    }

    [Fact]
    public void Get_ValidQuery_ReturnSuccessStatusCode()
    {
        // Act
        var actualResponse = _controller.Get();

        // Arrange
        using (new AssertionScope())
        {
            actualResponse.Should().BeAssignableTo<IQueryable<ContactResponse>>();
        }
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<IQueryable<ContactResponse>>)_controller.Get).Method;
        var permissions = new object[] { Common.Permissions.Contacts.Read, Common.Permissions.Contacts.ReadWrite };

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
    public async Task GetContactDetails_ExistingContactId_ReturnSuccessStatusCode()
    {
        // Arrange
        _contactServiceMock.Setup(p => p
                .GetContactDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new ContactDto());

        // Act
        var actualResponse = await _controller.GetContactDetailsAsync(new Guid(), default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<ContactResponse>();
        }
    }

    [Fact]
    public async Task GetContactDetails_NonExistingContactId_ReturnNotFoundStatusCode()
    {
        // Arrange
        _contactServiceMock.Setup(p => p
                .GetContactDetailsAsync(It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetContactDetailsAsync(new Guid(), default);

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
    public void GetContactDetails_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.GetContactDetailsAsync)
            .Method;
        var permissions = new object[] { Common.Permissions.Contacts.Read, Common.Permissions.Contacts.ReadWrite };

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
    public async Task ContactActivitiesHistory_ContactExists_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        _contactServiceMock.Setup(x =>
                x.GetContactActivitiesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new ActivityDto(0, new ActivityItemDto[] { }));

        // Act
        var actualResponse = await _controller.ContactActivitiesHistory
            (Guid.NewGuid(), new GetActivitiesRequest(1, 1), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<ActivityResponse>();
        }
    }

    [Fact]
    public async Task ContactActivitiesHistory_ContactDoesNotExist_ReturnsNotFoundStatusCode()
    {
        // Arrange
        _contactServiceMock.Setup(x =>
                x.GetContactActivitiesAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.ContactActivitiesHistory
            (Guid.NewGuid(), new GetActivitiesRequest(1, 1), default);

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
    public void ContactActivitiesHistory_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, GetActivitiesRequest, CancellationToken, Task<IActionResult>>)_controller
                .ContactActivitiesHistory).Method;
        var permissions = new object[] { Common.Permissions.Contacts.Read, Common.Permissions.Contacts.ReadWrite };

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
    public async Task UpdateContactAsync_NoConflictErrors_ReturnsContactResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(p => p.UpdateContactAsync(It.IsAny<Guid>(),
                It.IsAny<UpdateContactDto>(),
                default))
            .ReturnsAsync(new ContactDto());

        // Act
        var actualResponse = await _controller.UpdateContactsAsync(
            new Guid(),
            new UpdateAccountContactRequest(),
            CancellationToken.None);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<ContactResponse>();
        }
    }

    [Fact]
    public async Task UpdateContactAsync_ContactNotExists_ReturnsNotFoundResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(p => p.UpdateContactAsync(It.IsAny<Guid>(),
                It.IsAny<UpdateContactDto>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateContactsAsync(
            new Guid(),
            new UpdateContactRequest(),
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
    public void UpdateContactAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, UpdateContactRequest, CancellationToken, Task<IActionResult>>)
            _controller.UpdateContactsAsync).Method;
        var permissions = new object[] {
            Common.Permissions.Contacts.Read,
            Common.Permissions.Contacts.ReadWrite
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

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportContactsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportContactsRequest(fileType, "America/New_York");
        _contactServiceMock
            .Setup(x => x.ExportContactsAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportContactsAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"contacts.{fileType.ToString().ToLowerInvariant()}");
            actualResult.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Fact]
    public void ExportContactsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<ExportContactsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportContactsAsync)
            .Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Contacts.ReadExport");
        }
    }
}

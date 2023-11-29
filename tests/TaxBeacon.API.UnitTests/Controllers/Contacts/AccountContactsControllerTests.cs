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
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Errors;
using TaxBeacon.Common.Models;

namespace TaxBeacon.API.UnitTests.Controllers.Contacts;

public class AccountContactsControllerTests
{
    private readonly Mock<IContactService> _contactServiceMock;
    private readonly AccountContactsController _controller;

    public AccountContactsControllerTests()
    {
        _contactServiceMock = new();
        _controller = new AccountContactsController(_contactServiceMock.Object);
    }

    [Fact]
    public void Get_ValidQuery_ReturnSuccessStatusCode()
    {
        // Act
        var actualResponse = _controller.Get(Guid.NewGuid());

        // Arrange
        using (new AssertionScope())
        {
            actualResponse.Should().BeAssignableTo<IQueryable<AccountContactResponse>>();
        }
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, IQueryable<AccountContactResponse>>)_controller.Get).Method;
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
                .GetAccountContactDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(new AccountContactDto());

        // Act
        var actualResponse = await _controller.GetContactDetailsAsync(new Guid(), new Guid(), default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<AccountContactResponse>();
        }
    }

    [Fact]
    public async Task GetContactDetails_NonExistingContactId_ReturnNotFoundStatusCode()
    {
        // Arrange
        _contactServiceMock.Setup(p => p
                .GetAccountContactDetailsAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetContactDetailsAsync(new Guid(), new Guid(), default);

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
        var methodInfo = ((Func<Guid, Guid, CancellationToken, Task<IActionResult>>)_controller.GetContactDetailsAsync)
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
    public void UpdateContactStatusAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, Guid, Status, CancellationToken, Task<IActionResult>>)_controller.UpdateContactStatusAsync)
            .Method;
        var permissions = new object[] { Common.Permissions.Contacts.Activation };

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
    public async Task ExportAccountContactsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportContactsRequest(fileType, "America/New_York");
        _contactServiceMock
            .Setup(x => x.ExportAccountContactsAsync(
                It.IsAny<Guid>(),
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportAccountContactsAsync(Guid.NewGuid(), request, default);

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

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportAccountContactsAsync_AccountDoesNotExist_ReturnsNotFound(FileType fileType)
    {
        // Arrange
        var request = new ExportContactsRequest(fileType, "America/New_York");
        _contactServiceMock
            .Setup(x => x.ExportAccountContactsAsync(
                It.IsAny<Guid>(),
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.ExportAccountContactsAsync(Guid.NewGuid(), request, default);

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
    public void ExportAccountContactsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, ExportContactsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportAccountContactsAsync)
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

    [Fact]
    public async Task CreateNewContactAsync_ValidRequest_ReturnsContactDetailsResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.CreateNewContactAsync(It.IsAny<Guid>(),
                It.IsAny<CreateContactDto>(),
                default))
            .ReturnsAsync(new AccountContactDto());

        // Act
        var actualResponse = await _controller.CreateNewContactAsync(Guid.NewGuid(),
            new CreateContactRequest(),
            default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<AccountContactResponse>();
        }
    }

    [Fact]
    public async Task CreateNewContactAsync_AccountNotExists_ReturnsNotFoundResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.CreateNewContactAsync(It.IsAny<Guid>(),
                It.IsAny<CreateContactDto>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.CreateNewContactAsync(Guid.NewGuid(),
            new CreateContactRequest(),
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
    public void CreateNewContactAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, CreateContactRequest, CancellationToken, Task<IActionResult>>)
            _controller.CreateNewContactAsync).Method;
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
    public async Task AssignContactToAccountAsync_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.AssignContactToAccountAsync(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<ContactType>(),
                default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.AssignContactToAccountAsync(Guid.NewGuid(),
            Guid.NewGuid(),
            new AssignContactRequest(),
            default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
        }
    }

    [Fact]
    public async Task AssignContactToAccountAsync_AccountOrContactNotExists_ReturnsNotFoundResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.AssignContactToAccountAsync(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<ContactType>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.AssignContactToAccountAsync(Guid.NewGuid(),
            Guid.NewGuid(),
            new AssignContactRequest(),
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
    public async Task AssignContactToAccountAsync_ContactAlreadyAssignedToAccount_ReturnsConflictResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.AssignContactToAccountAsync(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<ContactType>(),
                default))
            .ReturnsAsync(new Conflict());

        // Act
        var actualResponse = await _controller.AssignContactToAccountAsync(Guid.NewGuid(),
            Guid.NewGuid(),
            new AssignContactRequest(),
            default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as ConflictObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status409Conflict);
            actualResult?.Value.Should().Be("Contact already assigned to this account");
        }
    }

    [Fact]
    public void AssignContactToAccountAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Guid, AssignContactRequest, CancellationToken, Task<IActionResult>>)
            _controller.AssignContactToAccountAsync).Method;
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
    public async Task UnassociateContactWithAccountAsync_ValidRequest_ReturnsSuccessResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.UnassociateContactWithAccount(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                default))
            .ReturnsAsync(new Success());

        // Act
        var actualResponse = await _controller.UnassociateContactWithAccountAsync(Guid.NewGuid(),
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
    public async Task UnassociateContactWithAccountAsync_AccountOrContactNotExists_ReturnsNotFoundResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.UnassociateContactWithAccount(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UnassociateContactWithAccountAsync(Guid.NewGuid(),
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
    public async Task UnassociateContactWithAccountAsync_ContactHasOnlyOneAccountLinked_ReturnsBadRequest()
    {
        // Arrange
        _contactServiceMock
            .Setup(s => s.UnassociateContactWithAccount(It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                default))
            .ReturnsAsync(new InvalidOperation("Message"));

        // Act
        var actualResponse = await _controller.UnassociateContactWithAccountAsync(Guid.NewGuid(),
            Guid.NewGuid(),
            default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as BadRequestObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }
    }

    [Fact]
    public async Task ContactActivitiesHistory_ContactExists_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        _contactServiceMock.Setup(x =>
                x.GetAccountContactActivitiesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(),
                    default))
            .ReturnsAsync(new ActivityDto(0, new ActivityItemDto[] { }));

        // Act
        var actualResponse = await _controller.ContactActivitiesHistory
            (Guid.NewGuid(), Guid.NewGuid(), new GetActivitiesRequest(1, 1), default);

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
                x.GetAccountContactActivitiesAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>(),
                    default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.ContactActivitiesHistory
            (Guid.NewGuid(), Guid.NewGuid(), new GetActivitiesRequest(1, 1), default);

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
            ((Func<Guid, Guid, GetActivitiesRequest, CancellationToken, Task<IActionResult>>)_controller
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
    public async Task UpdateAccountContactAsync_NoConflictErrors_ReturnsContactResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(p => p.UpdateAccountContactAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<UpdateAccountContactDto>(),
                default))
            .ReturnsAsync(new AccountContactDto());

        // Act
        var actualResponse = await _controller.UpdateAccountContactsAsync(
            new Guid(),
            new Guid(),
            new UpdateAccountContactRequest { Type = ContactType.Client },
            CancellationToken.None);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<AccountContactResponse>();
        }
    }

    [Fact]
    public async Task UpdateAccountContactAsync_ContactNotExists_ReturnsNotFoundResponse()
    {
        // Arrange
        _contactServiceMock
            .Setup(p => p.UpdateAccountContactAsync(It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<UpdateAccountContactDto>(),
                default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.UpdateAccountContactsAsync(
            new Guid(),
            new Guid(),
            new UpdateAccountContactRequest { Type = ContactType.Client },
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
    public void UpdateAccountContactAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<Guid, Guid, UpdateAccountContactRequest, CancellationToken, Task<IActionResult>>)
            _controller.UpdateAccountContactsAsync).Method;
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
}

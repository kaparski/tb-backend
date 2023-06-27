using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.Administration.Users;
using TaxBeacon.Administration.Users.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Users;
using TaxBeacon.API.Controllers.Users.Requests;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Errors;

namespace TaxBeacon.API.UnitTests.Controllers.User;

public class UsersControllerTest
{
    private readonly Mock<IUserService> _userServiceMock;
    private readonly UsersController _controller;

    public UsersControllerTest()
    {
        _userServiceMock = new();
        _controller = new UsersController(_userServiceMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { new Claim(Claims.TenantId, Guid.NewGuid().ToString()) }) })
                }
            }
        };
    }

    [Fact]
    public async Task GetUserList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email desc", };
        _userServiceMock.Setup(p => p.GetUsersAsync(query, default)).ReturnsAsync(
            new QueryablePaging<UserDto>(0,
                Enumerable.Empty<UserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetUserList(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<UserResponse>>();
        }
    }

    [Fact]
    public async Task GetUserList_InvalidQuery_ReturnBadRequest()
    {
        // Arrange
        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "username desc", };
        _userServiceMock.Setup(p => p.GetUsersAsync(query, default)).ReturnsAsync(
            new QueryablePaging<UserDto>(0,
                Enumerable.Empty<UserDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetUserList(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as BadRequestResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<BadRequestResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportUsersAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportUsersRequest(fileType, "America/New_York");
        _userServiceMock
            .Setup(x => x.ExportUsersAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportUsersAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"users.{fileType.ToString().ToLowerInvariant()}");
            actualResult!.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Fact]
    public async Task CreateUserAsync_ValidRequest_ReturnsCreatedStatusCode()
    {
        // Arrange
        var request = TestData.NewUser.Generate();
        _userServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), default))
            .ReturnsAsync(new UserDto());

        // Act
        var actualResponse = await _controller.CreateUserAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as CreatedResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<CreatedResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status201Created);
            actualResult?.Value.Should().BeOfType<UserResponse>();
        }
    }

    [Fact]
    public async Task CreateUserAsync_EmailExists_ReturnsConflictStatusCode()
    {
        // Arrange
        var request = TestData.NewUser.Generate();
        _userServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), default))
            .ReturnsAsync(new EmailAlreadyExists());

        // Act
        var actualResponse = await _controller.CreateUserAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as ConflictResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<ConflictResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        }
    }

    [Fact]
    public async Task CreateUserAsync_InvalidOperation_ReturnsBadRequestStatusCode()
    {
        // Arrange
        var request = TestData.NewUser.Generate();
        _userServiceMock
            .Setup(x => x.CreateUserAsync(It.IsAny<CreateUserDto>(), default))
            .ReturnsAsync(new InvalidOperation(string.Empty));

        // Act
        var actualResponse = await _controller.CreateUserAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as BadRequestObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<BadRequestObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
            actualResult?.Value.Should().BeOfType<string>();
        }
    }

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    private static class TestData
    {
        public static readonly Faker<CreateUserRequest> NewUser = new Faker<CreateUserRequest>()
            .CustomInstantiator(f => new CreateUserRequest(
                f.Name.FirstName(),
                f.Name.FirstName(),
                f.Name.LastName(),
                f.Internet.Email(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()));
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<IQueryable<UserResponse>>)_controller.Get).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Users.Read;Users.ReadWrite;Users.ReadExport");
        }
    }
}

﻿using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Accounts;
using TaxBeacon.API.Controllers.Accounts.Requests;
using TaxBeacon.API.Controllers.Accounts.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Enums.Accounts;

namespace TaxBeacon.API.UnitTests.Controllers.Accounts;

public sealed class AccountsControllerTests
{
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly AccountsController _controller;

    public AccountsControllerTests()
    {
        _accountServiceMock = new();
        _controller = new AccountsController(_accountServiceMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new[]
                    {
                        new ClaimsIdentity(new[] { new Claim(Claims.Permission, "Accounts.Read") })
                    })
                }
            }
        };
    }

    [Fact]
    public void Get_ReturnSuccessStatusCode()
    {
        // Act
        var actualResponse = _controller.Get();

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<IQueryable<AccountResponse>>();
        }
    }

    [Fact]
    public void Get_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<IActionResult>)_controller.Get).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Accounts.Read;Accounts.ReadWrite;Accounts.ReadExport");
        }
    }

    [Theory]
    [InlineData(FileType.Csv)]
    [InlineData(FileType.Xlsx)]
    public async Task ExportAccountsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportAccountsRequest(fileType, "America/New_York");
        _accountServiceMock
            .Setup(x => x.ExportAccountsAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportAccountsAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"accounts.{fileType.ToString().ToLowerInvariant()}");
            actualResult.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Fact]
    public void ExportAccountsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<ExportAccountsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportAccountsAsync)
            .Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be("Accounts.ReadExport");
        }
    }

    [Fact]
    public async Task GetAccountDetailsAsync_AccountExists_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        _accountServiceMock
            .Setup(x => x.GetAccountDetailsByIdAsync(It.IsAny<Guid>(), It.IsAny<AccountInfoType>(), default))
            .ReturnsAsync(new AccountDetailsDto());

        // Act
        var actualResponse = await _controller.GetAccountDetailsAsync(Guid.NewGuid(), default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<AccountDetailsResponse>();
        }
    }

    [Fact]
    public async Task GetAccountDetailsAsync_AccountDoesNotExist_ReturnsNotFoundStatusCode()
    {
        // Arrange
        _accountServiceMock
            .Setup(x => x.GetAccountDetailsByIdAsync(It.IsAny<Guid>(), It.IsAny<AccountInfoType>(), default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetAccountDetailsAsync(Guid.NewGuid(), default);

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
    public void GetAccountDetailsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<Guid, CancellationToken, Task<IActionResult>>)_controller.GetAccountDetailsAsync)
            .Method;

        var permissions = new object[]
        {
            Common.Permissions.Accounts.Read,
            Common.Permissions.Accounts.ReadWrite,
            Common.Permissions.Accounts.ReadExport,
            Common.Permissions.Clients.Read,
            Common.Permissions.Clients.ReadWrite,
            Common.Permissions.Clients.ReadExport,
            Common.Permissions.Referrals.Read,
            Common.Permissions.Referrals.ReadWrite,
            Common.Permissions.Referrals.ReadExport,
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

using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.ClientProspects;
using TaxBeacon.API.Controllers.ClientProspects.Requests;
using TaxBeacon.API.Controllers.ClientProspects.Responses;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.UnitTests.Controllers.ClientProspects;

public class ClientProspectControllerTest
{
    private readonly Mock<IAccountService> _accountServiceMock;
    private readonly ClientProspectsController _controller;

    public ClientProspectControllerTest()
    {
        _accountServiceMock = new();
        _controller = new ClientProspectsController(_accountServiceMock.Object);
    }

    [Fact]
    public void GetClientProspects_HasSuccessStatusCode()
    {
        // Arrange
        _accountServiceMock
            .Setup(p => p.QueryClientsProspects())
            .Returns(Enumerable.Empty<ClientProspectDto>().AsQueryable());

        // Act
        var actualResponse = _controller.GetClientProspects();

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<IQueryable<ClientProspectResponse>>();
        }
    }

    [Fact]
    public void GetClientProspects_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo = ((Func<IActionResult>)_controller.GetClientProspects).Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();
        var permissions = new object[]
        {
            Common.Permissions.Clients.Read,
            Common.Permissions.Accounts.Read,
        };

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
    public async Task ExportClientProspectAsync_ValidQuery_ReturnsFileContent(FileType fileType)
    {
        // Arrange
        var request = new ExportClientProspectRequest(fileType, "America/New_York");
        _accountServiceMock
            .Setup(x => x.ExportClientsProspectsAsync(
                It.IsAny<FileType>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<byte>());

        // Act
        var actualResponse = await _controller.ExportClientProspectsAsync(request, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            var actualResult = actualResponse as FileContentResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be($"client-prospects.{fileType.ToString().ToLowerInvariant()}");
            actualResult.ContentType.Should().Be(fileType switch
            {
                FileType.Csv => "text/csv",
                FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => throw new InvalidOperationException()
            });
        }
    }

    [Fact]
    public void ExportClientProspectsAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<ExportClientProspectRequest, CancellationToken, Task<IActionResult>>)_controller.ExportClientProspectsAsync)
            .Method;

        // Act
        var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

        var permissions = new object[]
        {
            Common.Permissions.Clients.ReadExport,
            Common.Permissions.Accounts.ReadExport,
        };

        // Assert
        using (new AssertionScope())
        {
            hasPermissionsAttribute.Should().NotBeNull();
            hasPermissionsAttribute?.Policy.Should().Be(string.Join(";", permissions.Select(x => $"{x.GetType().Name}.{x}")));
        }
    }
}

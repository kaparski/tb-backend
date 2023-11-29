using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.Accounts.Naics.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.NaicsCodes;
using TaxBeacon.API.Controllers.NaicsCodes.Responses;

namespace TaxBeacon.API.UnitTests.Controllers.Naics;
public class NaicsControllerTests
{
    private readonly Mock<INaicsService> _naicsServiceMock;
    private readonly NaicsController _controller;

    public NaicsControllerTests()
    {
        _naicsServiceMock = new Mock<INaicsService>();

        _controller = new NaicsController(_naicsServiceMock.Object)
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
    public void GetNaicsCodes_HasCorrectPermissions()
    {
        // Arrange
        var methodInfo = ((Func<CancellationToken, Task<IActionResult>>)_controller.GetNaicsCodesAsync).Method;
        var permissions = new object[]
        {
            Common.Permissions.Accounts.Read,
            Common.Permissions.Accounts.ReadWrite
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

    [Fact]
    public async Task GetNaicsCodes_ReturnSuccessStatusCode()
    {
        // Arrange
        _naicsServiceMock
            .Setup(x => x.GetNaicsCodesAsync(default))
            .ReturnsAsync(new List<NaicsCodeTreeItemDto>());

        // Act
        var actualResponse = await _controller.GetNaicsCodesAsync(default);

        // Assert
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeAssignableTo<List<NaicsCodeTreeItemResponse>>();
        }
    }
}

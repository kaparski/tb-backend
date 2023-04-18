using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Reflection;
using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Teams;
using TaxBeacon.API.Controllers.Teams.Requests;
using TaxBeacon.API.Controllers.Teams.Responses;
using TaxBeacon.API.Controllers.Tenants;
using TaxBeacon.API.Controllers.Tenants.Requests;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Divisions
{
    public class TenantDivisionControllerTest
    {
        private readonly Mock<ITenantDivisionsService> _tenantDivisionsServiceMock;
        private readonly TenantDivisionsController _controller;

        public TenantDivisionControllerTest()
        {
            _tenantDivisionsServiceMock = new();
            _controller = new TenantDivisionsController(_tenantDivisionsServiceMock.Object)
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
        public async Task GetTenantDivisionsList_ValidQuery_ReturnSuccessStatusCode()
        {
            // Arrange
            var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name desc", };
            _tenantDivisionsServiceMock.Setup(p => p.GetTenantDivisionsAsync(query, default)).ReturnsAsync(
                new QueryablePaging<DivisionDto>(0,
                    Enumerable.Empty<DivisionDto>().AsQueryable()));

            // Act
            var actualResponse = await _controller.GetTenantDivisionsList(query, default);

            // Arrange
            using (new AssertionScope())
            {
                var actualResult = actualResponse as OkObjectResult;
                actualResponse.Should().NotBeNull();
                actualResult.Should().NotBeNull();
                actualResponse.Should().BeOfType<OkObjectResult>();
                actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
                actualResult?.Value.Should().BeOfType<QueryablePaging<DivisionResponse>>();
            }
        }

        [Fact]
        public async Task GetTenantDivisionsList_InvalidQuery_ReturnBadRequest()
        {
            // Arrange
            var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "nonexistentfield desc", };
            _tenantDivisionsServiceMock.Setup(p => p.GetTenantDivisionsAsync(query, default)).ReturnsAsync(
                new QueryablePaging<DivisionDto>(0,
                    Enumerable.Empty<DivisionDto>().AsQueryable()));

            // Act
            var actualResponse = await _controller.GetTenantDivisionsList(query, default);

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
        public async Task ExportTeamsAsync_ValidQuery_ReturnsFileContent(FileType fileType)
        {
            // Arrange
            var request = new ExportTenantDivisionsRequest(fileType, "America/New_York");
            _tenantDivisionsServiceMock
                .Setup(x => x.ExportTenantDivisionsAsync(
                    It.IsAny<FileType>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<byte>());

            // Act
            var actualResponse = await _controller.ExportTenantsAsync(request, default);

            // Assert
            using (new AssertionScope())
            {
                actualResponse.Should().NotBeNull();
                var actualResult = actualResponse as FileContentResult;
                actualResult.Should().NotBeNull();
                actualResult!.FileDownloadName.Should().Be($"tenantDivisions.{fileType.ToString().ToLowerInvariant()}");
                actualResult!.ContentType.Should().Be(fileType switch
                {
                    FileType.Csv => "text/csv",
                    FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    _ => throw new InvalidOperationException()
                });
            }
        }

        [Fact]
        public void GetTenantDivisionsList_MarkedWithCorrectHasPermissionsAttribute()
        {
            // Arrange
            var methodInfo = ((Func<GridifyQuery, CancellationToken, Task<IActionResult>>)_controller.GetTenantDivisionsList).Method;

            // Act
            var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

            // Assert
            using (new AssertionScope())
            {
                hasPermissionsAttribute.Should().NotBeNull();
                hasPermissionsAttribute?.Policy.Should().Be("Tenants.Read;Tenants.ReadWrite;Tenants.ReadExport");
            }
        }

        [Fact]
        public void ExportTenantDivisionsAsync_MarkedWithCorrectHasPermissionsAttribute()
        {
            // Arrange
            var methodInfo = ((Func<ExportTenantDivisionsRequest, CancellationToken, Task<IActionResult>>)_controller.ExportTenantsAsync).Method;

            // Act
            var hasPermissionsAttribute = methodInfo.GetCustomAttribute<HasPermissions>();

            // Assert
            using (new AssertionScope())
            {
                hasPermissionsAttribute.Should().NotBeNull();
                hasPermissionsAttribute?.Policy.Should().Be("Tenants.ReadExport");
            }
        }
    }
}

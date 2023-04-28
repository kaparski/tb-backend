using FluentAssertions;
using FluentAssertions.Execution;
using Gridify;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Security.Claims;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.ServiceAreas;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Tenant;

public class ServiceAreasControllerTest
{
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly ServiceAreasController _controller;
    private readonly Guid _tenantId = Guid.NewGuid();

    public ServiceAreasControllerTest()
    {
        _tenantServiceMock = new();
        _currentUserServiceMock = new();

        _controller = new ServiceAreasController(_tenantServiceMock.Object, _currentUserServiceMock.Object)
        {
            ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new[] { new ClaimsIdentity(new[] { new Claim(Claims.TenantId, _tenantId.ToString()) }) })
                }
            }
        };
    }

    [Fact]
    public async Task GetServiceAreaList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(_tenantId);

        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name desc", };
        _tenantServiceMock.Setup(p => p.GetServiceAreasAsync(_tenantId, query, default)).ReturnsAsync(
            new QueryablePaging<ServiceAreaDto>(0,
                Enumerable.Empty<ServiceAreaDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetServiceAreaList(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<QueryablePaging<ServiceAreaResponse>>();
        }
    }

    [Fact]
    public async Task GetServiceAreaList_OrderByNonExistingProperty_ReturnBadRequestStatusCode()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(_tenantId);

        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "email desc", };
        _tenantServiceMock.Setup(p => p.GetServiceAreasAsync(_tenantId, query, default)).ReturnsAsync(
            new QueryablePaging<ServiceAreaDto>(0,
                Enumerable.Empty<ServiceAreaDto>().AsQueryable()));

        // Act
        var actualResponse = await _controller.GetServiceAreaList(query, default);

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

    [Fact]
    public async Task GetServiceAreaList_TenantIdDoesNotExist_ReturnNotFoundStatusCode()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(x => x.TenantId)
            .Returns(_tenantId);

        var query = new GridifyQuery { Page = 1, PageSize = 25, OrderBy = "name desc", };
        _tenantServiceMock.Setup(p => p.GetServiceAreasAsync(_tenantId, query, default))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.GetServiceAreaList(query, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<NotFoundResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }
}

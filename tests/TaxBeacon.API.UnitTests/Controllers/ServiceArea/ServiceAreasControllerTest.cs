using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaxBeacon.API.Controllers.ServiceAreas;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.Tenant;

public class ServiceAreasControllerTest
{
    private readonly Mock<ITenantService> _tenantServiceMock;
    private readonly ServiceAreasController _controller;

    public ServiceAreasControllerTest()
    {
        _tenantServiceMock = new();

        _controller = new ServiceAreasController(_tenantServiceMock.Object);
    }

    [Fact]
    public async Task GetServiceAreaList_ValidQuery_ReturnSuccessStatusCode()
    {
        // Arrange

        _tenantServiceMock
            .Setup(p => p.GetServiceAreasAsync(default))
            .ReturnsAsync(
                Enumerable.Empty<ServiceAreaDto>().ToArray()
            );

        // Act
        var actualResponse = await _controller.GetServiceAreaList(default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<List<ServiceAreaResponse>>();
        }
    }
}

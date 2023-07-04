using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaxBeacon.API.Controllers.GlobalSearch;
using TaxBeacon.API.Controllers.GlobalSearch.Requests;
using TaxBeacon.API.Controllers.GlobalSearch.Responses;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.UnitTests.Controllers.GlobalSearch;

public class GlobalSearchControllerTests
{
    private readonly Mock<IGlobalSearchService> _serviceMock = new();
    private readonly SearchController _controller;

    public GlobalSearchControllerTests() => _controller = new SearchController(_serviceMock.Object);

    [Fact]
    public async Task SearchAsync_ValidRequest_ReturnsSuccessfulStatusCode()
    {
        // Arrange
        var request = new Faker<SearchRequest>()
            .CustomInstantiator(f => new SearchRequest(f.Name.FirstName(), 1, 10))
            .Generate();
        _serviceMock.Setup(x => x.SearchAsync(
                It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), default))
            .ReturnsAsync(new SearchResultsDto());

        // Act
        var actualResponse = await _controller.SearchAsync(request, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<SearchResultsResponse>();
        }
    }
}

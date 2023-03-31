using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Mapster;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using TaxBeacon.API.Controllers.TableFilters;
using TaxBeacon.API.Controllers.TableFilters.Requests;
using TaxBeacon.API.Controllers.TableFilters.Responses;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Erros;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.UnitTests.Controllers.TableFilters;

public class TableFiltersControllerTests
{
    private readonly Mock<ITableFiltersService> _tableFiltersServiceMock;
    private readonly TableFiltersController _controller;

    public TableFiltersControllerTests()
    {
        _tableFiltersServiceMock = new();

        _controller = new TableFiltersController(_tableFiltersServiceMock.Object);
    }

    [Fact]
    private async Task GetFiltersAsync_ValidQuery_ReturnsListOfFilters()
    {
        // Arrange
        var tableType = new Faker().PickRandom<EntityType>();
        var tableFilterDto = TestData.TableFilterDtoFaker.Generate(5);

        _tableFiltersServiceMock
            .Setup(s => s.GetFiltersAsync(
                It.Is<EntityType>(et => et == tableType),
                It.Is<CancellationToken>(ct => ct == default)))
            .ReturnsAsync(tableFilterDto);

        // Act
        var actualResponse = await _controller.GetFiltersAsync(tableType, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<List<TableFilterResponse>>();
        }
    }

    [Fact]
    private async Task CreateFilterAsync_ValidCreateTableFilterRequest_ReturnsListOfFilters()
    {
        // Arrange
        var tableFilterDto = TestData.TableFilterDtoFaker.Generate();
        var createTableFilterRequest = TestData.CreateTableFilterRequestFaker.Generate();
        var createTableFilterDto = createTableFilterRequest.Adapt<CreateTableFilterDto>();

        _tableFiltersServiceMock
            .Setup(s => s.CreateFilterAsync(
                It.Is<CreateTableFilterDto>(r =>
                    r.Name.Equals(createTableFilterDto.Name, StringComparison.OrdinalIgnoreCase)),
                It.Is<CancellationToken>(ct => ct == default)))
            .ReturnsAsync(Enumerable.Empty<TableFilterDto>().ToList());

        // Act
        var actualResponse = await _controller.CreateFilterAsync(createTableFilterRequest, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<List<TableFilterResponse>>();
        }
    }

    [Fact]
    private async Task CreateFilterAsync_AlreadyExistsName_ReturnsConflictResponse()
    {
        // Arrange
        var createTableFilterRequest = TestData.CreateTableFilterRequestFaker.Generate();
        var createTableFilterDto = createTableFilterRequest.Adapt<CreateTableFilterDto>();

        _tableFiltersServiceMock
            .Setup(s => s.CreateFilterAsync(
                It.Is<CreateTableFilterDto>(r =>
                    r.Name.Equals(createTableFilterDto.Name, StringComparison.OrdinalIgnoreCase)),
                It.Is<CancellationToken>(ct => ct == default)))
            .ReturnsAsync(new NameAlreadyExists());

        // Act
        var actualResponse = await _controller.CreateFilterAsync(createTableFilterRequest, default);

        // Arrange
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
    private async Task DeleteFilterAsync_ValidFilterId_ReturnsListOfFilters()
    {
        // Arrange
        var filterId = Guid.NewGuid();
        _tableFiltersServiceMock
            .Setup(s => s.DeleteFilterAsync(
                It.Is<Guid>(id => id == filterId),
                It.Is<CancellationToken>(ct => ct == default)))
            .ReturnsAsync(Enumerable.Empty<TableFilterDto>().ToList());

        // Act
        var actualResponse = await _controller.DeleteFilterAsync(filterId, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as OkObjectResult;
            actualResponse.Should().NotBeNull();
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<OkObjectResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status200OK);
            actualResult?.Value.Should().BeOfType<List<TableFilterResponse>>();
        }
    }

    [Fact]
    private async Task DeleteFilterAsync_InvalidFilterId_ReturnsNotFoundResponse()
    {
        // Arrange
        var filterId = Guid.NewGuid();
        _tableFiltersServiceMock
            .Setup(s => s.DeleteFilterAsync(
                It.Is<Guid>(id => id == filterId),
                It.Is<CancellationToken>(ct => ct == default)))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller.DeleteFilterAsync(filterId, default);

        // Arrange
        using (new AssertionScope())
        {
            var actualResult = actualResponse as NotFoundResult;
            actualResult.Should().NotBeNull();
            actualResponse.Should().BeOfType<NotFoundResult>();
            actualResult?.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
    }

    private static class TestData
    {
        public static readonly Faker<TableFilterDto> TableFilterDtoFaker =
            new Faker<TableFilterDto>()
                .RuleFor(tf => tf.Id, _ => Guid.NewGuid())
                .RuleFor(tf => tf.Name, f => f.Hacker.Noun())
                .RuleFor(tf => tf.Configuration, f => f.Lorem.Text());

        public static readonly Faker<CreateTableFilterRequest> CreateTableFilterRequestFaker =
            new Faker<CreateTableFilterRequest>()
                .CustomInstantiator(f =>
                    new CreateTableFilterRequest(f.Hacker.Noun(), f.Lorem.Text(), f.PickRandom<EntityType>()));
    }
}

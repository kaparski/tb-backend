using Bogus;
using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OneOf.Types;
using System.Net.Mime;
using System.Reflection;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Templates;
using TaxBeacon.API.Controllers.Templates.Requests;
using TaxBeacon.Common.Enums;
using TaxBeacon.DocumentManagement.BlobStorage.Models;
using TaxBeacon.DocumentManagement.Templates;

namespace TaxBeacon.API.UnitTests.Controllers.Templates;

public class TemplatesControllerTests
{
    private readonly Mock<ITemplatesService> _templatesServiceMock;
    private readonly TemplatesController _controller;

    public TemplatesControllerTests()
    {
        _templatesServiceMock = new();
        _controller = new TemplatesController(_templatesServiceMock.Object);
    }

    [Fact]
    public async Task GetImportTemplatesAsync_TemplateTypeHasDeclaredFilesUrls_ReturnsFile()
    {
        // Arrange
        var stream = new Mock<Stream>();
        var getImportTemplatesRequest = new GetImportTemplatesRequest { TemplateType = TemplateType.Contacts };
        const string expectedFileName = "Test.zip";
        _templatesServiceMock
            .Setup(s => s.GetTemplateAsync(It.IsAny<TemplateType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DownloadFileResultDto(stream.Object, expectedFileName, MediaTypeNames.Application.Zip));

        // Act
        var actualResponse = await _controller
            .GetImportTemplatesAsync(getImportTemplatesRequest, default);

        // Assert
        using (new AssertionScope())
        {
            actualResponse.Should().NotBeNull();
            actualResponse.Should().BeAssignableTo<FileStreamResult>();
            var actualResult = actualResponse as FileStreamResult;
            actualResult.Should().NotBeNull();
            actualResult!.FileDownloadName.Should().Be(expectedFileName);
            actualResult.ContentType.Should().Be(MediaTypeNames.Application.Zip);
        }
    }

    [Fact]
    public async Task GetImportTemplatesAsync_TemplateTypeDoesntHaveDeclaredFilesUrls_ReturnsNotFound()
    {
        // Arrange
        var getImportTemplatesRequest = new GetImportTemplatesRequest { TemplateType = TemplateType.Contacts };
        _templatesServiceMock
            .Setup(s => s.GetTemplateAsync(It.IsAny<TemplateType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new NotFound());

        // Act
        var actualResponse = await _controller
            .GetImportTemplatesAsync(getImportTemplatesRequest, default);

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
    public void GetImportTemplatesAsync_MarkedWithCorrectHasPermissionsAttribute()
    {
        // Arrange
        var methodInfo =
            ((Func<GetImportTemplatesRequest, CancellationToken, Task<IActionResult>>)_controller
                .GetImportTemplatesAsync)
            .Method;
        var permissions = new object[] { Common.Permissions.Templates.Read };

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

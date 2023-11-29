using FluentAssertions;
using FluentAssertions.Execution;
using Microsoft.Extensions.Logging;
using Moq;
using OneOf.Types;
using System.Net.Mime;
using TaxBeacon.Common.Enums;
using TaxBeacon.DocumentManagement.BlobStorage;
using TaxBeacon.DocumentManagement.BlobStorage.Models;
using TaxBeacon.DocumentManagement.Templates;

namespace TaxBeacon.DocumentManagement.UnitTests.Templates;

public class TemplatesServiceTests
{
    private readonly Mock<IBlobStorageService> _blobStorageServiceMock;
    private readonly ITemplatesService _templatesService;

    public TemplatesServiceTests()
    {
        Mock<ILogger<TemplatesService>> templatesServiceLoggerMock = new();
        _blobStorageServiceMock = new();

        _templatesService = new TemplatesService(_blobStorageServiceMock.Object, templatesServiceLoggerMock.Object);
    }

    [Theory]
    [MemberData(nameof(TestData.ZipArchiveTemplateTypes), MemberType = typeof(TestData))]
    public async Task GetTemplateAsync_TemplateTypeHasDeclaredFilesUrls_ReturnsZipArchive(TemplateType templateType)
    {
        // Arrange
        var streamMock = new Mock<Stream>();

        _blobStorageServiceMock
            .Setup(s => s.DownloadFilesAsync(It.IsAny<string>(), It.IsAny<string[]>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[]
            {
                new DownloadFileResultDto(streamMock.Object, "test.txt", MediaTypeNames.Application.Pdf)
            });

        // Act
        var actualResult = await _templatesService.GetTemplateAsync(templateType);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT0(out var zipArchive, out _).Should().BeTrue();
            zipArchive.Should().NotBeNull();
            zipArchive.Should().BeOfType<DownloadFileResultDto>();
            zipArchive.FileName.Should().Be("Import template.zip");
            zipArchive.ContentType.Should().Be(MediaTypeNames.Application.Zip);
        }
    }

    [Fact]
    public async Task GetTemplateAsync_TemplateTypeDoesntHaveDeclaredFilesUrls_ReturnsNotFound()
    {
        // Act
        var actualResult = await _templatesService.GetTemplateAsync(TemplateType.None);

        // Assert
        using (new AssertionScope())
        {
            actualResult.TryPickT1(out var notFound, out _).Should().BeTrue();
            notFound.Should().BeOfType<NotFound>();
        }
    }

    private static class TestData
    {
        public static IEnumerable<object[]> ZipArchiveTemplateTypes = Enum.GetValues<TemplateType>()
            .Where(t => t != TemplateType.None)
            .Select(t => new object[] { t });
    }
}

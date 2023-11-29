using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using System.IO.Compression;
using System.Net.Mime;
using TaxBeacon.Common.Enums;
using TaxBeacon.DocumentManagement.BlobStorage;
using TaxBeacon.DocumentManagement.BlobStorage.Models;

namespace TaxBeacon.DocumentManagement.Templates;

public class TemplatesService: ITemplatesService
{
    private const string ContainerName = "templates";
    private const string ArchiveName = "Import template.zip";

    private readonly IReadOnlyDictionary<TemplateType, string[]> _templatesUrls =
        new Dictionary<TemplateType, string[]>
        {
            {
                TemplateType.Contacts,
                new[] { "contacts/Contacts - import template.csv", "contacts/Contacts import instructions.txt" }
            },
            {
                TemplateType.Entities,
                new[] { "entities/Entities - import template.csv", "entities/Entities import instructions.txt" }
            },
            {
                TemplateType.Locations,
                new[] { "locations/Locations - import template.csv", "locations/Locations import instructions.txt" }
            },
            {
                TemplateType.StateIds,
                new[] { "stateIds/State IDs - import template.csv", "stateIds/State IDs import instructions.txt" }
            }
        };

    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<TemplatesService> _logger;

    public TemplatesService(IBlobStorageService blobStorageService, ILogger<TemplatesService> logger)
    {
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    public async Task<OneOf<DownloadFileResultDto, NotFound>> GetTemplateAsync(TemplateType templateType,
        CancellationToken cancellationToken = default)
    {
        if (!_templatesUrls.TryGetValue(templateType, out var templateUrls))
        {
            return new NotFound();
        }

        if (templateUrls.Length == 1)
        {
            return await _blobStorageService.DownloadFileAsync(ContainerName, templateUrls.First(), cancellationToken);
        }

        var files = await _blobStorageService.DownloadFilesAsync(ContainerName, templateUrls, cancellationToken);
        var memoryStream = new MemoryStream();

        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in files)
            {
                var archiveItem = archive.CreateEntry(file.FileName);
                await using var archiveItemStream = archiveItem.Open();
                await file.Stream.CopyToAsync(archiveItemStream, cancellationToken);
            }
        }

        memoryStream.Position = 0;
        return new DownloadFileResultDto(memoryStream, ArchiveName, MediaTypeNames.Application.Zip);
    }
}

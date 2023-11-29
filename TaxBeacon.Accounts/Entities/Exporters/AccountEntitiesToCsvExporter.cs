using System.Collections.Immutable;
using System.IO.Compression;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Exporters;
public class AccountEntitiesToCsvExporter: IAccountEntitiesToCsvExporter
{
    private readonly IListToFileConverter _listToFileConverter;
    public AccountEntitiesToCsvExporter(IEnumerable<IListToFileConverter> listToFileConverters) => _listToFileConverter = listToFileConverters.First(x => x.FileType == FileType.Csv);

    public async Task<FileStreamDto> Export(AccountEntitiesExportModel model, CancellationToken cancellationToken)
    {
        var memoryStream = new MemoryStream();
        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var entitiesCsvFile = archive.CreateEntry("entities.csv");
            await using (var entryStream = entitiesCsvFile.Open())
            {
                var csv = _listToFileConverter.Convert(model.Entities);
                await entryStream.WriteAsync(csv, cancellationToken);
            }
            var locationsCsvFile = archive.CreateEntry("locations.csv");
            await using (var entryStream = locationsCsvFile.Open())
            {
                var csv = _listToFileConverter.Convert(model.Locations);
                await entryStream.WriteAsync(csv, cancellationToken);
            }
            var stateIdsCsvFile = archive.CreateEntry("stateIds.csv");
            await using (var entryStream = stateIdsCsvFile.Open())
            {
                var csv = _listToFileConverter.Convert(model.StateIds);
                await entryStream.WriteAsync(csv, cancellationToken);
            }
        }
        memoryStream.Position = 0;
        return new FileStreamDto(memoryStream, "entities.zip", FileType.Zip);
    }
}

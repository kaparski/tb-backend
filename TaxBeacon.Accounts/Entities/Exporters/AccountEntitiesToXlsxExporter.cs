using Npoi.Mapper;
using System.IO;
using System.IO.Compression;
using System.Text;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Entities.Exporters;
public class AccountEntitiesToXlsxExporter: IAccountEntitiesToXlsxExporter
{
    public FileStreamDto Export(AccountEntitiesExportModel model)
    {
        var book = new NPOI.XSSF.UserModel.XSSFWorkbook();
        var mapper = new Mapper(book);

        mapper.Put(model.Entities, "Entities");
        mapper.Put(model.StateIds, "State IDs");
        mapper.Put(model.Locations, "Locations");

        var stream = new MemoryStream();
        book.Write(stream, true);
        stream.Seek(0, SeekOrigin.Begin);

        return new FileStreamDto(stream, "entities.xlsx", FileType.Xlsx);
    }
}

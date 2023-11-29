using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace TaxBeacon.Accounts.Common.Services;

public class CsvService: ICsvService
{
    private readonly CsvConfiguration _config = new(CultureInfo.InvariantCulture) { HasHeaderRecord = true };

    public IEnumerable<T> Read<T>(Stream stream)
    {
        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, _config);
        return csv.GetRecords<T>().ToList();
    }
}

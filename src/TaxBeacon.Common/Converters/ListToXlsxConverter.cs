using Npoi.Mapper;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Common.Converters;

public sealed class ListToXlsxConverter: IListToFileConverter
{
    public FileType FileType => FileType.Xlsx;

    public byte[] Convert<T>(List<T> data) => ((MemoryStream)ConvertStream(data)).ToArray();

    public Stream ConvertStream<T>(List<T> data)
    {
        var mapper = new Mapper();

        using var stream = new MemoryStream();
        mapper.Save(stream, data);

        return stream;
    }
}

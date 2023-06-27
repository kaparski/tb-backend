using TaxBeacon.Common.Enums;

namespace TaxBeacon.Common.Converters;

public interface IListToFileConverter
{
    public FileType FileType { get; }

    byte[] Convert<T>(List<T> data);
}
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Common.Converters;

public static class FileTypeToMimeTypeConverter
{
    public static string ToMimeType(this FileType fileType) => fileType switch
    {
        FileType.Csv => "text/csv",
        FileType.Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        FileType.Zip => "application/zip",
        _ => throw new InvalidOperationException(),
    };
}

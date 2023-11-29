using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Common.Models;
public class FileStreamDto
{
    public FileStreamDto(Stream stream, string? fileName, FileType fileType)
    {
        FileStream = stream;
        FileName = fileName;
        ContentType = fileType.ToMimeType();
    }

    public string? FileName { get; }
    public Stream FileStream { get; }
    public string ContentType { get; }
}

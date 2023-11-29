namespace TaxBeacon.DocumentManagement.BlobStorage.Models;

public record DownloadFileResultDto(Stream Stream, string FileName, string ContentType);

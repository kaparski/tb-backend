using TaxBeacon.DocumentManagement.BlobStorage.Models;

namespace TaxBeacon.DocumentManagement.BlobStorage;

public interface IBlobStorageService
{
    Task<UploadFileResultDto> UploadFileAsync(UploadFileDto uploadFileDto,
        CancellationToken cancellationToken = default);

    Task<UploadFileResultDto[]> UploadFilesAsync(UploadFileDto[] documentDtos,
        CancellationToken cancellationToken = default);

    Task<DownloadFileResultDto> DownloadFileAsync(string containerName,
        string url,
        CancellationToken cancellationToken = default);

    Task<DownloadFileResultDto[]> DownloadFilesAsync(string containerName,
        string[] filesUrls,
        CancellationToken cancellationToken = default);
}

using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using TaxBeacon.Common.Services;
using TaxBeacon.DocumentManagement.BlobStorage.Models;

namespace TaxBeacon.DocumentManagement.BlobStorage;

public class BlobStorageService: IBlobStorageService
{
    private const long FileSizeRestriction = 256 * 1024 * 1024;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(BlobServiceClient blobServiceClient,
        ICurrentUserService currentUserService,
        IDateTimeService dateTimeService,
        ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
        _logger = logger;
    }

    public async Task<UploadFileResultDto> UploadFileAsync(UploadFileDto uploadFileDto,
        CancellationToken cancellationToken = default)
    {
        var containerName = _currentUserService.TenantId.ToString();
        var containerClient = await GetOrCreateBlobContainerClientAsync(containerName, cancellationToken);

        var uploadDocumentResult = await UploadFileAsync(containerClient, uploadFileDto, cancellationToken);

        _logger.LogInformation("{dateTime} - File ({fileId} was uploaded by {userId}",
            _dateTimeService.UtcNow,
            uploadDocumentResult.FileId,
            _currentUserService.UserId);

        return uploadDocumentResult;
    }

    public async Task<UploadFileResultDto[]> UploadFilesAsync(UploadFileDto[] documentDtos,
        CancellationToken cancellationToken = default)
    {
        var containerName = _currentUserService.TenantId.ToString();
        var containerClient = await GetOrCreateBlobContainerClientAsync(containerName, cancellationToken);
        var uploadTasks = documentDtos
            .Select(document => UploadFileAsync(containerClient, document, cancellationToken));

        try
        {
            var uploadFilesResult = await Task.WhenAll(uploadTasks);

            _logger.LogInformation("{dateTime} - File ({filesIds} were uploaded by {userId}",
                _dateTimeService.UtcNow,
                string.Join(", ", uploadFilesResult.Select(r => r.FileId)),
                _currentUserService.UserId);

            return uploadFilesResult;
        }
        catch (Exception)
        {
            foreach (var uploadDocumentDto in documentDtos)
            {
                var blobClient = containerClient.GetBlobClient(GetFileName(uploadDocumentDto));
                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            }

            throw;
        }
    }

    public async Task<DownloadFileResultDto> DownloadFileAsync(string containerName,
        string url,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var downloadFileResult = await DownloadFileAsync(containerClient, url, cancellationToken);

        _logger.LogInformation("{dateTime} - File ({fileUrl} was downloaded by {userId}",
            _dateTimeService.UtcNow,
            url,
            _currentUserService.UserId);

        return downloadFileResult;
    }

    public async Task<DownloadFileResultDto[]> DownloadFilesAsync(string containerName,
        string[] filesUrls,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        var downloadTasks = filesUrls
            .Select(url => DownloadFileAsync(containerClient, url, cancellationToken));

        var downloadFilesResult = await Task.WhenAll(downloadTasks);

        _logger.LogInformation("{dateTime} - Files ({filesUrls} were downloaded by {userId}",
            _dateTimeService.UtcNow,
            string.Join(", ", filesUrls),
            _currentUserService.UserId);

        return downloadFilesResult;
    }

    private async Task<BlobContainerClient> GetOrCreateBlobContainerClientAsync(string containerName,
        CancellationToken cancellationToken = default)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);

        return await containerClient.ExistsAsync(cancellationToken)
            ? containerClient
            : await _blobServiceClient
                .CreateBlobContainerAsync(containerName, cancellationToken: cancellationToken);
    }

    private async Task<UploadFileResultDto> UploadFileAsync(BlobContainerClient blobContainerClient,
        UploadFileDto uploadFileDto,
        CancellationToken cancellationToken = default)
    {
        var fileName = GetFileName(uploadFileDto);

        var blobClient = blobContainerClient.GetBlobClient(fileName);
        await using var stream = uploadFileDto.File.OpenReadStream();
        BlobUploadOptions? blobUploadOptions = null;

        if (uploadFileDto.File.Length > FileSizeRestriction)
        {
            using var md5 = MD5.Create();
            var hash = await md5.ComputeHashAsync(stream, cancellationToken);

            stream.Position = 0;
            blobUploadOptions = new BlobUploadOptions { HttpHeaders = new BlobHttpHeaders { ContentHash = hash } };
        }

        await blobClient.UploadAsync(stream, blobUploadOptions, cancellationToken);

        return new UploadFileResultDto(uploadFileDto.DocumentId, fileName);
    }

    private async Task<DownloadFileResultDto> DownloadFileAsync(BlobContainerClient blobContainerClient,
        string url,
        CancellationToken cancellationToken  = default)
    {
        var blobClient = blobContainerClient.GetBlobClient(url);

        var downloadOptions = new BlobOpenReadOptions(false)
        {
            TransferValidation = new DownloadTransferValidationOptions
            {
                AutoValidateChecksum = true, ChecksumAlgorithm = StorageChecksumAlgorithm.Auto
            }
        };

        var stream = await blobClient.OpenReadAsync(downloadOptions, cancellationToken);
        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

        return new DownloadFileResultDto(stream, url.Split('/').Last(), properties.Value.ContentType);
    }

    private string GetFileName(UploadFileDto uploadFileDto) =>
        $"{uploadFileDto.EntityType}/{uploadFileDto.DocumentId}/{uploadFileDto.File.FileName}";
}

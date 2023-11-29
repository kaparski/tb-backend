using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.FeatureManagement;
using TaxBeacon.DocumentManagement;
using TaxBeacon.DocumentManagement.BlobStorage;
using TaxBeacon.DocumentManagement.BlobStorage.Models;

namespace TaxBeacon.API.Controllers.QualityAssurance;

[Authorize]
[HasPermissions(Common.Permissions.QualityAssurance.Full)]
public class QualityAssuranceController: BaseController
{
    private readonly IFeatureFlagsService _featureFlagsService;
    private readonly IBlobStorageService _blobStorageService;

    public QualityAssuranceController(IFeatureFlagsService featureFlagsService, IBlobStorageService blobStorageService)
    {
        _featureFlagsService = featureFlagsService;
        _blobStorageService = blobStorageService;
    }

    /// <summary>
    /// Endpoint for getting the current values of feature flags
    /// </summary>
    /// <response code="200">Returns current values of feature flags</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Feature flags values</returns>
    [HttpGet(Name = "GetFeatureFlagValues")]
    [ProducesResponseType(typeof(IDictionary<string, bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> GetCurrentFeatureFlagsAsync() =>
        Task.FromResult<IActionResult>(Ok(_featureFlagsService.GetCurrentFeatureFlags()));

    /// <summary>
    /// Endpoint for updating feature flag values
    /// </summary>
    /// <response code="200">Returns feature flags values</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Feature flags values</returns>
    [HttpPatch(Name = "UpdateFeatureFlagValues")]
    [ProducesResponseType(typeof(IDictionary<string, bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> UpdateFeatureFlagsAsync([FromBody] IDictionary<string, bool> newFlags)
    {
        _featureFlagsService.UpdateFeatureFlag(newFlags);

        return Task.FromResult<IActionResult>(Ok(_featureFlagsService.GetCurrentFeatureFlags()));
    }

    /// <summary>
    /// Endpoint for resetting feature flag values
    /// </summary>
    /// <response code="200">Returns feature flags values</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Feature flags values</returns>
    [HttpDelete(Name = "ResetFeatureFlagValues")]
    [ProducesResponseType(typeof(IDictionary<string, bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public Task<IActionResult> ResetFeatureFlagsAsync()
    {
        _featureFlagsService.ResetFeatureFlags();

        return Task.FromResult<IActionResult>(Ok(_featureFlagsService.GetCurrentFeatureFlags()));
    }

    /// <summary>
    /// Endpoint for uploading single file
    /// </summary>
    /// <response code="200">Returns uploaded document uri</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Uploaded file uri</returns>
    [HttpPost("files", Name = "UploadFile")]
    [ProducesResponseType(typeof(UploadFileResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        var uploadResult = await _blobStorageService
            .UploadFileAsync(new UploadFileDto(Guid.NewGuid(), EntityType.Account, file), default);

        return Ok(uploadResult);
    }

    /// <summary>
    /// Endpoint for downloading single file
    /// </summary>
    /// <response code="200">Returns downloaded document</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Downloaded file</returns>
    [HttpGet("files", Name = "DownloadFile")]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DownloadFile(string url)
    {
        var file = await _blobStorageService.DownloadFileAsync(url, default);

        return File(file.Stream, file.ContentType, file.FileName);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Exceptions;
using TaxBeacon.API.Controllers.Documents.Responses;
using TaxBeacon.API.Controllers.Documents.Requests;
using TaxBeacon.Accounts.Documents;
using Mapster;
using FluentValidation.Results;
using TaxBeacon.Common.Converters;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.Controllers.Documents;

[Authorize]
/// <response code="401">User is unauthorized</response>
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
/// <response code="403">The user does not have the required permission</response>
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[FeatureGate(FeatureFlagKeys.Documents)]
public class DocumentsController: BaseController
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService) => _documentService = documentService;

    /// <summary>
    /// Get documents of an account
    /// </summary>
    /// <response code="200">Returns account's documents</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Account is not found</response>
    /// <returns>A collection of documents associated with a particular account</returns>
    [HasPermissions(Common.Permissions.Documents.Read, Common.Permissions.Documents.ReadExport)]
    [EnableQuery]
    [HttpGet("api/accounts/{accountId:guid}/documents")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<DocumentResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get([FromRoute] Guid accountId)
    {
        var result = _documentService.QueryDocuments(accountId);

        return result.Match<IActionResult>(
            query => Ok(query.ProjectToType<DocumentResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Export list of account's documents
    /// </summary>
    /// <response code="200">Returns a file</response>
    /// <response code="400">Validation failed</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Account with specified Id is not found</response>
    /// <returns>File</returns>
    [HasPermissions(Common.Permissions.Documents.ReadExport)]
    [HttpGet("/api/accounts/{accountId:guid}/documents/export", Name = "ExportDocuments")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportDocumentsAsync([FromRoute] Guid accountId,
        [FromQuery] ExportDocumentsRequest exportRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportRequest.FileType.ToMimeType();

        var result = await _documentService.ExportDocumentsAsync(accountId, exportRequest.FileType, cancellationToken);

        return result.Match<IActionResult>(
            bytes => File(bytes, mimeType, $"documents.{exportRequest.FileType.ToString().ToLowerInvariant()}"),
            _ => NotFound());
    }
}

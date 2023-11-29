using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Templates.Requests;
using TaxBeacon.API.Exceptions;
using TaxBeacon.DocumentManagement.Templates;

namespace TaxBeacon.API.Controllers.Templates;

[Authorize]
public class TemplatesController: BaseController
{
    private readonly ITemplatesService _templatesService;

    public TemplatesController(ITemplatesService templatesService) => _templatesService = templatesService;

    /// <summary>
    /// Returns import templates for specific type
    /// </summary>
    /// <response code="200">Returns a file</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Template type doesn't have specified template urls</response>
    /// <returns>Import templates</returns>
    [HasPermissions(Common.Permissions.Templates.Read)]
    [HttpGet(Name = "GetImportTemplates")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImportTemplatesAsync(
        [FromQuery] GetImportTemplatesRequest getImportTemplatesRequest,
        CancellationToken cancellationToken)
    {
        var result = await _templatesService
            .GetTemplateAsync(getImportTemplatesRequest.TemplateType, cancellationToken);

        return result.Match<IActionResult>(
            file => File(file.Stream, file.ContentType, file.FileName),
            _ => NotFound());
    }
}

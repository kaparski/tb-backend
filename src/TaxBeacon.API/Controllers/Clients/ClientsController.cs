using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.ClientProspects.Requests;
using TaxBeacon.API.Controllers.Clients.Requests;
using TaxBeacon.API.Controllers.Clients.Responses;
using TaxBeacon.Common.Converters;

namespace TaxBeacon.API.Controllers.Clients;

public class ClientsController: BaseController
{
    private readonly IAccountService _accountService;

    public ClientsController(IAccountService accountService) => _accountService = accountService;

    /// <summary>
    /// Get clients
    /// </summary>
    /// <response code="200">Returns clients</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Get client</returns>
    [HasPermissions(Common.Permissions.Clients.Read, Common.Permissions.Accounts.Read)]
    [EnableQuery]
    [HttpGet("api/accounts/clients", Name = "GetClients")]
    [ProducesResponseType(typeof(IQueryable<ClientResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetClients()
    {
        var clients = _accountService.QueryClients();
        return Ok(clients.ProjectToType<ClientResponse>());
    }

    /// <summary>
    /// Export clients
    /// </summary>
    /// <response code="200">Export clients</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Export client</returns>
    [HasPermissions(Common.Permissions.Clients.ReadExport, Common.Permissions.Accounts.ReadExport)]
    [HttpGet("/api/accounts/clients/export", Name = "ExportClients")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportClientAsync([FromQuery] ExportClientRequest request,
        CancellationToken cancellationToken)
    {
        var mimeType = request.FileType.ToMimeType();

        var clientProspects = await _accountService.ExportClientsAsync(
            request.FileType, cancellationToken);

        return File(clientProspects, mimeType,
            $"clients.{request.FileType.ToString().ToLowerInvariant()}");
    }
}

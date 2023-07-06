using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Accounts.Responses;

namespace TaxBeacon.API.Controllers.ClientProspects;

public class ClientProspectsController: BaseController
{
    private readonly IAccountService _accountService;

    public ClientProspectsController(IAccountService accountService) => _accountService = accountService;

    /// <summary>
    /// Get client prospects
    /// </summary>
    /// <response code="200">Returns client prospects</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Get client referrals</returns>
    [HasPermissions(Common.Permissions.Clients.Read, Common.Permissions.Accounts.Read)]
    [EnableQuery]
    [HttpGet("api/accounts/clientprospects", Name = "GetClientProspects")]
    [ProducesResponseType(typeof(IQueryable<ClientProspectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetClientProspectsAsync()
    {
        var clientProspects = _accountService.QueryClientsProspects();
        return Ok(clientProspects.ProjectToType<ClientProspectResponse>());
    }
}

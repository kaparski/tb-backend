using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Accounts.Responses;
using TaxBeacon.API.Exceptions;

namespace TaxBeacon.API.Controllers.Accounts;

public class AccountsController: BaseController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService) => _accountService = accountService;

    /// <summary>
    /// Queryable list of accounts (as OData endpoint).
    /// </summary>
    /// <response code="200">Returns users</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of users</returns>
    // [HasPermissions(
    //     Common.Permissions.Accounts.Read,
    //     Common.Permissions.Accounts.ReadWrite,
    //     Common.Permissions.Accounts.ReadExport)]
    [EnableQuery]
    [HttpGet("api/odata/accounts")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<AccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<AccountResponse> Get()
    {
        var query = _accountService.GetAccounts();

        return query.ProjectToType<AccountResponse>();
    }
}

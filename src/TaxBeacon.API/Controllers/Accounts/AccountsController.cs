using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Accounts.Requests;
using TaxBeacon.API.Controllers.Accounts.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Accounts;

public class AccountsController: BaseController
{
    private readonly IAccountService _accountService;

    public AccountsController(IAccountService accountService) => _accountService = accountService;

    /// <summary>
    /// Queryable list of accounts (as OData endpoint).
    /// </summary>
    /// <response code="200">Returns accounts</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of users</returns>
    [HasPermissions(
        Common.Permissions.Accounts.Read,
        Common.Permissions.Accounts.ReadWrite,
        Common.Permissions.Accounts.ReadExport)]
    [EnableQuery]
    [HttpGet("api/odata/accounts")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<AccountResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Get() => Ok(_accountService.QueryAccounts().ProjectToType<AccountResponse>());

    /// <summary>
    /// Endpoint to export accounts
    /// </summary>
    /// <param name="exportAccountsRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Accounts.ReadExport)]
    [HttpGet("export", Name = "ExportAccounts")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportAccountsAsync(
        [FromQuery] ExportAccountsRequest exportAccountsRequest,
        CancellationToken cancellationToken)
    {
        var mimeType = exportAccountsRequest.FileType.ToMimeType();

        var accounts = await _accountService.ExportAccountsAsync(
            exportAccountsRequest.FileType, cancellationToken);

        return File(accounts, mimeType,
            $"accounts.{exportAccountsRequest.FileType.ToString().ToLowerInvariant()}");
    }

    /// <summary>
    /// Get account By Id
    /// </summary>
    /// <response code="200">Returns Account Details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Account is not found</response>
    /// <returns>Account details</returns>
    [HasPermissions(
        Common.Permissions.Accounts.Read,
        Common.Permissions.Accounts.ReadWrite,
        Common.Permissions.Accounts.ReadExport,
        Common.Permissions.Clients.Read,
        Common.Permissions.Clients.ReadWrite,
        Common.Permissions.Clients.ReadExport,
        Common.Permissions.Referrals.Read,
        Common.Permissions.Referrals.ReadWrite,
        Common.Permissions.Referrals.ReadExport)]
    [HttpGet("{id:guid}", Name = "AccountDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(AccountDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAccountDetailsAsync([FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var accountsPermissions = Enum.GetValues<Common.Permissions.Accounts>();
        var clientPermissions = Enum.GetValues<Common.Permissions.Clients>();
        var referralPermissions = Enum.GetValues<Common.Permissions.Clients>();
        var accountInfoType = AccountInfoType.None;

        if (User.HasAnyPermission(accountsPermissions))
        {
            accountInfoType = AccountInfoType.Full;
        }
        else if (User.HasAnyPermission(clientPermissions) && !User.HasAnyPermission(referralPermissions))
        {
            accountInfoType = AccountInfoType.Client;
        }
        else if (User.HasAnyPermission(referralPermissions) && !User.HasAnyPermission(clientPermissions))
        {
            accountInfoType = AccountInfoType.Referral;
        }

        var getAccountDetailsResult =
            await _accountService.GetAccountDetailsByIdAsync(id, accountInfoType, cancellationToken);

        return getAccountDetailsResult.Match<IActionResult>(
            result => Ok(result.Adapt<AccountDetailsResponse>()),
            _ => NotFound());
    }
}

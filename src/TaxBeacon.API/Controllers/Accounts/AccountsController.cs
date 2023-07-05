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
using TaxBeacon.Common.Enums.Accounts;

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
        var accountInfoType = GetAccountInfoTypeByPermissions();

        var getAccountDetailsResult =
            await _accountService.GetAccountDetailsByIdAsync(id, accountInfoType, cancellationToken);

        return getAccountDetailsResult.Match<IActionResult>(
            result => Ok(result.Adapt<AccountDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Get Account's Activity History
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Account is not found</response>
    /// <returns>Activity history for a specific account</returns>
    [HasPermissions(
        Common.Permissions.Accounts.Read,
        Common.Permissions.Accounts.ReadWrite)]
    [HttpGet("{id:guid}/activities", Name = "AccountActivityHistory")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<AccountActivityHistoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivitiesHistory([FromRoute] Guid id,
        [FromQuery] AccountActivityHistoryRequest request,
        CancellationToken cancellationToken)
    {
        var accountInfoType = GetAccountInfoTypeByPermissions();
        var activities = await _accountService.GetActivityHistoryAsync(id,
            accountInfoType,
            request.Page,
            request.PageSize,
            cancellationToken);

        return activities.Match<IActionResult>(
            result => Ok(result.Adapt<AccountActivityHistoryResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Update client status
    /// </summary>
    /// <param name="accountId">Account id</param>
    /// <param name="clientStatus">New client status</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns updated client</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Updated client</returns>
    [HasPermissions(Common.Permissions.Clients.Activation)]
    [HttpPatch("{accountId:guid}/client/status", Name = "UpdateClientStatus")]
    [ProducesResponseType(typeof(AccountDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateClientStatusAsync([FromRoute] Guid accountId,
        [FromBody] Status clientStatus,
        CancellationToken cancellationToken)
    {
        var accountInfoType = GetAccountInfoTypeByPermissions();
        var updatedStatusResult = await _accountService.UpdateClientStatusAsync(accountId,
            clientStatus,
            accountInfoType,
            cancellationToken);

        return updatedStatusResult.Match<IActionResult>(
            user => Ok(user.Adapt<AccountDetailsResponse>()),
            _ => NotFound());
    }

    private AccountInfoType GetAccountInfoTypeByPermissions()
    {
        var accountsPermissions = Enum.GetValues<Common.Permissions.Accounts>();
        var clientPermissions = Enum.GetValues<Common.Permissions.Clients>();
        var referralPermissions = Enum.GetValues<Common.Permissions.Clients>();

        if (User.HasAnyPermission(accountsPermissions))
        {
            return AccountInfoType.Full;
        }

        if (User.HasAnyPermission(clientPermissions) && !User.HasAnyPermission(referralPermissions))
        {
            return AccountInfoType.Client;
        }

        if (User.HasAnyPermission(referralPermissions) && !User.HasAnyPermission(clientPermissions))
        {
            return AccountInfoType.Referral;
        }

        return AccountInfoType.General;
    }
}

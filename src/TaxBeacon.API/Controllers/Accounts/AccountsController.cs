using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.Accounts.Accounts.Models;
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
    [HasPermissions(Common.Permissions.Clients.Activation, Common.Permissions.Prospects.Activation)]
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

    /// <summary>
    /// Update client details
    /// </summary>
    /// <response code="200">Returns updated account</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Client is not found</response>
    /// <returns>Updated account client</returns>
    [HasPermissions(Common.Permissions.Clients.ReadWrite, Common.Permissions.Accounts.ReadWrite)]
    [HttpPut("{accountId:guid}/client", Name = "UpdateClient")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(AccountDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateClientAsync([FromRoute] Guid accountId,
        [FromBody] UpdateClientRequest request, CancellationToken cancellationToken)
    {
        var resultOneOf =
            await _accountService.UpdateClientDetailsAsync(accountId, request.Adapt<UpdateClientDto>(), cancellationToken);

        return resultOneOf.Match<IActionResult>(
            result => Ok(result.Adapt<AccountDetailsResponse>()),
            notFound => NotFound());
    }

    /// <summary>
    /// Update account profile details
    /// </summary>
    /// <param name="accountId">Account id</param>
    /// <param name="updateAccountProfileRequest">Updated account profile</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns updated account details</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Updated account details</returns>
    [HasPermissions(Common.Permissions.Accounts.ReadWrite)]
    [HttpPatch("{accountId:guid}", Name = "UpdateAccountProfile")]
    [ProducesResponseType(typeof(AccountDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(InvalidOperationResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateAccountProfileAsync([FromRoute] Guid accountId,
        [FromBody] UpdateAccountProfileRequest updateAccountProfileRequest,
        CancellationToken cancellationToken)
    {
        var updatedAccountProfileResult = await _accountService.UpdateAccountProfileAsync(accountId,
            updateAccountProfileRequest.Adapt<UpdateAccountProfileDto>(),
            cancellationToken);

        return updatedAccountProfileResult.Match<IActionResult>(
            user => Ok(user.Adapt<AccountDetailsResponse>()),
            notFound => NotFound(),
            error => BadRequest(error.Adapt<InvalidOperationResponse>()));
    }

    /// <summary>
    /// Create account
    /// </summary>
    /// <param name="newAccount">New client account</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns created client account</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Created client account</returns>
    [HasPermissions(
        Common.Permissions.Clients.ReadWrite,
        Common.Permissions.Accounts.ReadWrite,
        Common.Permissions.Referrals.ReadWrite)]
    [HttpPost(Name = "CreateAccount")]
    [ProducesResponseType(typeof(AccountDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateAccountAsync(
        [FromBody] CreateAccountRequest newAccount,
        CancellationToken cancellationToken)
    {
        var accountInfoType = GetAccountInfoTypeByPermissions();
        var createAccountResult = await _accountService.CreateAccountAsync(newAccount.Adapt<CreateAccountDto>(),
            accountInfoType,
            cancellationToken);

        return createAccountResult.Match<IActionResult>(
            user => Ok(user.Adapt<AccountDetailsResponse>()),
            error => BadRequest(error.Adapt<InvalidOperationResponse>()));
    }

    /// <summary>
    /// Generate unique AccountId for tenantId
    /// </summary>
    /// <response code="200">Unique Account Id</response>
    /// <response code="400">Failed to generate unique id</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>AccountId</returns>
    [HasPermissions(Common.Permissions.Accounts.ReadWrite)]
    //TODO: route `~/api/accounts/generateUniqueAccountId` conflicts with odata
    [HttpGet("~/api/account/generate-unique-account-id", Name = "GenerateUniqueAccountId")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GenerateUniqueAccountId(CancellationToken cancellationToken)
    {
        var result =
            await _accountService.GenerateUniqueAccountIdAsync(cancellationToken);

        return result.Match<IActionResult>(
            code => Ok(code),
            error => BadRequest(error.Message));
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

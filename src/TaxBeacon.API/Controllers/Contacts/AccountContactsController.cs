using FluentValidation.Results;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Identity.Client;
using TaxBeacon.Accounts.Contacts;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Contacts.Requests;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.API.Shared.Requests;
using TaxBeacon.API.Shared.Responses;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.Controllers.Contacts;

[Authorize]
[FeatureGate(FeatureFlagKeys.Contacts)]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public class AccountContactsController: BaseController
{
    private readonly IContactService _contactService;

    public AccountContactsController(IContactService contactService) => _contactService = contactService;

    /// <summary>
    /// Queryable list of contacts (as OData endpoint).
    /// </summary>
    /// <response code="200">Returns users</response>
    /// <response code="400">Invalid filtering or sorting</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>List of contacts</returns>
    [HasPermissions(
        Common.Permissions.Contacts.Read,
        Common.Permissions.Contacts.ReadWrite)]
    [EnableQuery]
    [HttpGet("api/odata/accounts/{accountId:guid}/accountcontacts", Name = "GetAccountContacts")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<AccountContactResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<AccountContactResponse> Get(Guid accountId) =>
        _contactService
            .QueryAccountContacts(accountId)
            .ProjectToType<AccountContactResponse>();

    /// <summary>
    /// Get Contact Details
    /// </summary>
    /// <response code="200">Returns contact</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Not found</response>
    /// <returns>Contact Details</returns>
    [HasPermissions(
        Common.Permissions.Contacts.Read,
        Common.Permissions.Contacts.ReadWrite)]
    [HttpGet("/api/accounts/{accountId:guid}/contacts/{contactId:guid}", Name = "GetAccountContactDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(AccountContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContactDetailsAsync([FromRoute] Guid accountId, [FromRoute] Guid contactId,
        CancellationToken cancellationToken)
    {
        var getContactDetailsResult = await _contactService
            .GetAccountContactDetailsAsync(accountId, contactId, cancellationToken);

        return getContactDetailsResult.Match<IActionResult>(
            contacts => Ok(contacts.Adapt<AccountContactResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Endpoint to update contact status
    /// </summary>
    /// <param name="accountId">Account id</param>
    /// <param name="contactId">Contact id</param>
    /// <param name="contactStatus">New contact status</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns updated contact</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Contact was not found</response>
    /// <returns>Updated contact</returns>
    [HasPermissions(Common.Permissions.Contacts.Activation)]
    [HttpPut("/api/accounts/{accountId:guid}/contacts/{contactId:guid}/status", Name = "UpdateContactStatus")]
    [ProducesResponseType(typeof(AccountContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContactStatusAsync([FromRoute] Guid accountId,
        [FromRoute] Guid contactId,
        [FromBody] Status contactStatus,
        CancellationToken cancellationToken)
    {
        var updatedStatusResult =
            await _contactService.UpdateAccountContactStatusAsync(accountId, contactId, contactStatus,
                cancellationToken);

        return updatedStatusResult.Match<IActionResult>(
            user => Ok(user.Adapt<AccountContactResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Endpoint for creating a new contact
    /// </summary>
    /// <param name="accountId">Account id</param>
    /// <param name="createContactRequest">New contact data</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns created contact</response>
    /// <response code="400">Invalid request</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">An account with that id doesn't exist</response>
    /// <returns>Created contact details</returns>
    [HasPermissions(Common.Permissions.Contacts.ReadWrite)]
    [HttpPost("/api/accounts/{accountId:guid}/contacts", Name = "CreateNewContact")]
    [ProducesResponseType(typeof(AccountContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateNewContactAsync([FromRoute] Guid accountId,
        [FromBody] CreateContactRequest createContactRequest,
        CancellationToken cancellationToken)
    {
        var createContactResult = await _contactService
            .CreateNewContactAsync(accountId, createContactRequest.Adapt<CreateContactDto>(), cancellationToken);

        return createContactResult.Match<IActionResult>(
            createdContact => Ok(createdContact.Adapt<AccountContactResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Endpoint for assigning a contact to an account
    /// </summary>
    /// <param name="accountId">Account id</param>
    /// <param name="contactId">Contact id</param>
    /// <param name="assignContactRequest">Request that contains contact type</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Success response</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">An account with that id doesn't exist or a contact with that id doesn't exist</response>
    /// <response code="409">Contact already assigned to this account</response>
    /// <returns>Success response</returns>
    [HasPermissions(Common.Permissions.Contacts.ReadWrite)]
    [HttpPost("/api/accounts/{accountId:guid}/contacts/{contactId:guid}", Name = "AssignContactToAccount")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignContactToAccountAsync(
        [FromRoute] Guid accountId,
        [FromRoute] Guid contactId,
        [FromBody] AssignContactRequest assignContactRequest,
        CancellationToken cancellationToken)
    {
        var assignContactResult = await _contactService
            .AssignContactToAccountAsync(accountId, contactId, assignContactRequest.Type, cancellationToken);

        return assignContactResult.Match<IActionResult>(
            _ => Ok(),
            _ => NotFound(),
            _ => Conflict("Contact already assigned to this account"));
    }

    /// <summary>
    /// Endpoint for unassociating the contact from the account
    /// </summary>
    /// <param name="accountId">Account id</param>
    /// <param name="contactId">Contact id</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Success response</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">An account with that id doesn't exist or a contact with that id doesn't exist</response>
    /// <response code="409">Contact already assigned to this account</response>
    /// <returns>Success response</returns>
    [HasPermissions(Common.Permissions.Contacts.ReadWrite)]
    [HttpDelete("/api/accounts/{accountId:guid}/contacts/{contactId:guid}", Name = "UnassociateContactWithAccount")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [FeatureGate(FeatureFlagKeys.ContactsUnassociateAccount)]
    public async Task<IActionResult> UnassociateContactWithAccountAsync(
        [FromRoute] Guid accountId,
        [FromRoute] Guid contactId,
        CancellationToken cancellationToken)
    {
        var assignContactResult = await _contactService
            .UnassociateContactWithAccount(accountId: accountId, contactId, cancellationToken);

        return assignContactResult.Match<IActionResult>(
            _ => Ok(),
            _ => NotFound(),
            err => BadRequest(err.Adapt<InvalidOperationResponse>()));
    }

    /// <summary>
    /// Endpoint to export contacts
    /// </summary>
    /// <param name="accountId"></param>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Account with specified Id is not found</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Contacts.ReadExport)]
    [HttpGet("/api/accounts/{accountId:guid}/contacts/export", Name = "ExportAccountContacts")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportAccountContactsAsync([FromRoute] Guid accountId,
        [FromQuery] ExportContactsRequest request,
        CancellationToken cancellationToken)
    {
        var mimeType = request.FileType.ToMimeType();

        var contacts = await _contactService.ExportAccountContactsAsync(accountId,
            request.FileType, cancellationToken);

        return contacts.Match<IActionResult>(
            bytes => File(bytes, mimeType, $"contacts.{request.FileType.ToString().ToLowerInvariant()}"),
            _ => NotFound());
    }

    /// <summary>
    /// Get account contact activity history
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Contact or association with account was not found</response>
    /// <returns>Activity history for a specific account contact</returns>
    [HasPermissions(Common.Permissions.Contacts.Read, Common.Permissions.Contacts.ReadWrite)]
    [HttpGet("/api/accounts/{accountId:guid}/contacts/{contactId:guid}/activities", Name = "GetAccountContactActivityHistory")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<ActivityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ContactActivitiesHistory([FromRoute] Guid accountId,
        [FromRoute] Guid contactId,
        [FromQuery] GetActivitiesRequest request,
        CancellationToken cancellationToken)
    {
        var activities = await _contactService
            .GetAccountContactActivitiesAsync(accountId, contactId, request.Page, request.PageSize, cancellationToken);

        return activities.Match<IActionResult>(
            result => Ok(result.Adapt<ActivityResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Update Account Contact Details
    /// </summary>
    /// <response code="200">Returns contact</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Not found</response>
    /// <returns>Account Contact Details</returns>
    [HasPermissions(
        Common.Permissions.Contacts.Read,
        Common.Permissions.Contacts.ReadWrite)]
    [HttpPatch("/api/accounts/{accountId:guid}/contacts/{contactId:guid}", Name = "UpdateAccountContactDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(AccountContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAccountContactsAsync([FromRoute] Guid contactId,
        [FromRoute] Guid accountId,
        [FromBody] UpdateAccountContactRequest request,
        CancellationToken cancellationToken)
    {
        var contactDetails = await _contactService
            .UpdateAccountContactAsync(contactId, accountId, request.Adapt<UpdateAccountContactDto>(), cancellationToken);

        return contactDetails.Match<IActionResult>(
            contacts => Ok(contacts.Adapt<AccountContactResponse>()),
            _ => NotFound());
    }
}

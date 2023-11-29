using FluentValidation.Results;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Contacts;
using TaxBeacon.Accounts.Contacts.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Contacts.Requests;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.API.Shared.Requests;
using TaxBeacon.API.Shared.Responses;
using TaxBeacon.Common.FeatureManagement;
using TaxBeacon.Common.Converters;

namespace TaxBeacon.API.Controllers.Contacts;

[Authorize]
[FeatureGate(FeatureFlagKeys.Contacts)]
public class ContactsController: BaseController
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService) => _contactService = contactService;

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
    [HttpGet("api/odata/contacts")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<ContactResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<ContactResponse> Get() =>
        _contactService
            .QueryContacts()
            .ProjectToType<ContactResponse>();

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
    [HttpGet("{contactId:guid}", Name = "GetContactDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContactDetailsAsync([FromRoute] Guid contactId,
        CancellationToken cancellationToken)
    {
        var getContactDetailsResult = await _contactService
            .GetContactDetailsAsync(contactId, cancellationToken);

        return getContactDetailsResult.Match<IActionResult>(
            contacts => Ok(contacts.Adapt<ContactResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Get contact activity history
    /// </summary>
    /// <response code="200">Returns activity logs</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Contact was not found</response>
    /// <returns>Activity history for a specific contact</returns>
    [HasPermissions(Common.Permissions.Contacts.Read, Common.Permissions.Contacts.ReadWrite)]
    [HttpGet("{contactId:guid}/activities", Name = "GetContactActivityHistory")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<ActivityResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ContactActivitiesHistory([FromRoute] Guid contactId,
        [FromQuery] GetActivitiesRequest request,
        CancellationToken cancellationToken)
    {
        var activities = await _contactService
            .GetContactActivitiesAsync(contactId, request.Page, request.PageSize, cancellationToken);

        return activities.Match<IActionResult>(
            result => Ok(result.Adapt<ActivityResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Update Contact Details
    /// </summary>
    /// <response code="200">Returns contact</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Not found</response>
    /// <returns>Contact Details</returns>
    [HasPermissions(
        Common.Permissions.Contacts.Read,
        Common.Permissions.Contacts.ReadWrite)]
    [HttpPatch("{contactId:guid}", Name = "UpdateContactDetails")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(IEnumerable<ValidationFailure>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateContactsAsync([FromRoute] Guid contactId,
        [FromBody] UpdateContactRequest request,
        CancellationToken cancellationToken)
    {
        var contactDetails = await _contactService
            .UpdateContactAsync(contactId, request.Adapt<UpdateContactDto>(), cancellationToken);

        return contactDetails.Match<IActionResult>(
            contacts => Ok(contacts.Adapt<ContactResponse>()),
                 _ => NotFound());
    }

    /// <summary>
    /// Export list of contacts
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Contacts.ReadExport)]
    [HttpGet("~/api/contacts/export", Name = "ExportContacts")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportContactsAsync(
        [FromQuery] ExportContactsRequest request,
        CancellationToken cancellationToken)
    {
        var mimeType = request.FileType.ToMimeType();

        var contacts = await _contactService.ExportContactsAsync(
            request.FileType, cancellationToken);

        return File(contacts, mimeType,
            $"contacts.{request.FileType.ToString().ToLowerInvariant()}");
    }
}

using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Contacts;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Contacts.Requests;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.Controllers.Contacts;

[Authorize]
[FeatureGate(FeatureFlagKeys.Contacts)]
public class LinkedContactsController: BaseController
{
    private readonly IContactService _contactService;

    public LinkedContactsController(IContactService contactService) => _contactService = contactService;

    /// <summary>
    /// Get Contact Linked Contacts
    /// </summary>
    /// <response code="200">Returns contact linked contacts</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Not found</response>
    /// <returns>Contact linked contacts</returns>
    [HasPermissions(Common.Permissions.Contacts.Read, Common.Permissions.Contacts.ReadWrite,
        Common.Permissions.Contacts.ReadExport)]
    [EnableQuery]
    [HttpGet(Name = "GetContactLinkedContacts")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<LinkedContactDetailsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] Guid contactId,
        CancellationToken cancellationToken)
    {
        var getContactDetailsResult = await _contactService
            .QueryLinkedContacts(contactId, cancellationToken);

        return getContactDetailsResult.Match<IActionResult>(
            contacts => Ok(contacts.ProjectToType<LinkedContactDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Endpoint for unlinking the contact from another contact
    /// </summary>
    /// <param name="contactId">Source contact id</param>
    /// <param name="relatedContactId">Related contact id</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Success response</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">One or both the contacts with such ids doesn't exist</response>
    /// <response code="409">Contacts already unlinked</response>
    /// <returns>Success response</returns>
    [HasPermissions(Common.Permissions.Contacts.ReadWrite)]
    [HttpDelete("~/api/contacts/{contactId:guid}/[controller]/{relatedContactId:guid}",
        Name = "UnlinkContactFromContact")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnlinkContactFromContactAsync([FromRoute] Guid contactId,
        [FromRoute] Guid relatedContactId,
        CancellationToken cancellationToken)
    {
        var assignContactResult = await _contactService
            .UnlinkContactFromContactAsync(contactId, relatedContactId, cancellationToken);

        return assignContactResult.Match<IActionResult>(
            _ => Ok(),
            _ => NotFound());
    }

    /// <summary>
    /// Endpoint for linking the contact to another contact
    /// </summary>
    /// <param name="contactId">Source contact id</param>
    /// <param name="linkContactRequest">Request that contains related contact id and comment</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Success response</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">One or both the contacts with such ids doesn't exist</response>
    /// <response code="409">Contacts already unlinked</response>
    /// <returns>Success response</returns>
    [HasPermissions(Common.Permissions.Contacts.ReadWrite)]
    [HttpPost("~/api/contacts/{contactId:guid}/[controller]", Name = "LinkContactToContact")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LinkContactToContactAsync([FromRoute] Guid contactId,
        [FromBody] LinkContactRequest linkContactRequest,
        CancellationToken cancellationToken)
    {
        var linkContactResult = await _contactService
            .LinkContactToContactAsync(contactId,
                linkContactRequest.RelatedContactId,
                linkContactRequest.Comment,
                cancellationToken);

        return linkContactResult.Match<IActionResult>(
            _ => Ok(),
            _ => NotFound(),
            _ => Conflict());
    }
}

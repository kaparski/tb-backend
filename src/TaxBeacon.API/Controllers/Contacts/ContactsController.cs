using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Services.Contacts;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.API.Exceptions;

namespace TaxBeacon.API.Controllers.Contacts;

[Authorize]
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
        Common.Permissions.Contacts.Read)]
    [EnableQuery]
    [HttpGet("api/accounts/{accountId:guid}/contacts")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<ContactResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Get([FromRoute] Guid accountId)
    {
        var oneOf = await _contactService.QueryContactsAsync(accountId);

        return oneOf.Match<IActionResult>(
            contacts => Ok(contacts.Value.ProjectToType<ContactResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Get Contact Details
    /// </summary>
    /// <response code="200">Returns contact</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <response code="404">Not found</response>
    /// <returns>Contact Details</returns>
    [HasPermissions(
        Common.Permissions.Contacts.Read)]
    [HttpGet("/api/accounts/{accountId:guid}/contacts/{contactId:guid}")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(ContactDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContactDetails([FromRoute] Guid accountId, [FromRoute] Guid contactId)
    {
        var oneOf = await _contactService.GetContactDetailsAsync(accountId, contactId);

        return oneOf.Match<IActionResult>(
            contacts => Ok(contacts.Adapt<ContactDetailsResponse>()),
            _ => NotFound());
    }
}

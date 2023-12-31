﻿using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Contacts;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Contacts.Requests;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Converters;

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
        Common.Permissions.Contacts.Read,
        Common.Permissions.Contacts.ReadWrite)]
    [EnableQuery]
    [HttpGet("api/odata/contacts")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<ContactResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<ContactResponse> Get()
    {
        var dtos = _contactService.QueryContacts();

        return dtos.ProjectToType<ContactResponse>();
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
        Common.Permissions.Contacts.Read,
        Common.Permissions.Contacts.ReadWrite)]
    [HttpGet("/api/accounts/{accountId:guid}/contacts/{contactId:guid}")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(ContactDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContactDetails([FromRoute] Guid accountId, [FromRoute] Guid contactId, CancellationToken cancellationToken)
    {
        var oneOf = await _contactService.GetContactDetailsAsync(contactId, accountId, cancellationToken);

        return oneOf.Match<IActionResult>(
            contacts => Ok(contacts.Adapt<ContactDetailsResponse>()),
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
    /// <returns>Updated contact</returns>
    [HasPermissions(Common.Permissions.Contacts.Activation)]
    [HttpPut("/api/accounts/{accountId:guid}/contacts/{contactId:guid}/status", Name = "UpdateContactStatus")]
    [ProducesResponseType(typeof(ContactDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateContactStatusAsync([FromRoute] Guid accountId,
        [FromRoute] Guid contactId,
        [FromBody] Status contactStatus,
        CancellationToken cancellationToken)
    {
        var updatedStatusResult = await _contactService.UpdateContactStatusAsync(contactId, accountId, contactStatus, cancellationToken);

        return updatedStatusResult.Match<IActionResult>(
            user => Ok(user.Adapt<ContactDetailsResponse>()),
            _ => NotFound());
    }

    /// <summary>
    /// Endpoint to export contacts
    /// </summary>
    /// <param name="request"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Returns file content</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>File content</returns>
    [HasPermissions(Common.Permissions.Contacts.ReadExport)]
    [HttpGet("export", Name = "ExportContacts")]
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

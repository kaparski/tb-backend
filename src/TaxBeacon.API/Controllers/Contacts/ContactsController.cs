using Mapster;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.API.Exceptions;
using TaxBeacon.UserManagement.Services.Contacts;

namespace TaxBeacon.API.Controllers.Contacts;

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
    [HttpGet("/api/odata/contacts", Name = "GetContacts")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IQueryable<ContactResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IQueryable<ContactResponse> GetContacts()
    {
        var query = _contactService.QueryContacts();

        return query.ProjectToType<ContactResponse>();
    }
}

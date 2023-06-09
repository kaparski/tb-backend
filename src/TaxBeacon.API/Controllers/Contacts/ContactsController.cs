using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.UserManagement.Services.Contacts;

namespace TaxBeacon.API.Controllers.Contacts;

public class ContactsController
{
    private readonly IContactService _contactService;

    public ContactsController(IContactService contactService) => _contactService = contactService;

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContactResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetContactAsync(Guid id)
    {
        var contacts = await _contactService.GetContactAsync(id);

        return contacts is null
            ? new NotFoundResult()
            : new OkObjectResult(contacts);
    }
}

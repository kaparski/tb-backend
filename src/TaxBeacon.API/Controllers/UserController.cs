using Microsoft.AspNetCore.Mvc;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers;

public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService) => _userService = userService;

    // GET
    [HttpGet]
    public IActionResult Get() => Ok();
}

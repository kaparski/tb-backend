using Gridify;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.UserManagement.Models;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Users;

[Route("api/users")]
public class UserController: BaseController
{
    private readonly IUserService _userService;

    public UserController(IUserService userService) => _userService = userService;

    /// <summary>
    /// List of users
    /// </summary>
    /// <remarks>
    /// Sample request:
    ///
    ///     GET /users
    ///     {
    ///     "page":1,
    ///     "pageSize":2,
    ///     "orderBy":"username desc",
    ///     "filter":"firstName=Dastan"
    ///     }
    /// </remarks>
    /// <response code="200">Returns users</response>
    /// <returns>List of users</returns>
    [HttpGet(Name = "GetUsers")]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<UserList>), StatusCodes.Status200OK)]
    public ActionResult<QueryablePaging<UserList>> GetList([FromQuery] GridifyQuery query)
    {
        var users = _userService.GetUsers(query);
        return users;
    }
}

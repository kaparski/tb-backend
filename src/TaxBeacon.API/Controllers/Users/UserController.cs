using Gridify;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.Common.Enums;
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
    [ProducesResponseType(typeof(QueryablePaging<UserListResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<QueryablePaging<UserListResponse>>> GetList([FromQuery] GridifyQuery query, CancellationToken cancellationToken)
    {
        var users = await _userService.GetUsersAsync(query, cancellationToken);
        var userListResponse = new QueryablePaging<UserListResponse>(users.Count, users.Query.ProjectToType<UserListResponse>());

        return userListResponse;
    }
}

using Gridify;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.Users;

[Route("api/users")]
public class UsersController: BaseController
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService) => _userService = userService;

    /// <summary>
    /// List of users
    /// </summary>
    /// <remarks>
    /// Sample request: <br/><br/>
    ///     ```GET /users/page=1&amp;pageSize=21&amp;orderBy="username desc"&amp;filter="firstName=Dastan"/```
    /// </remarks>
    /// <response code="200">Returns users</response>
    /// <returns>List of users</returns>
    [HttpGet(Name = "GetUsers")]
    [ProducesDefaultResponseType(typeof(ProblemDetails))]
    [ProducesResponseType(typeof(QueryablePaging<UserResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserList([FromQuery] GridifyQuery query, CancellationToken cancellationToken)
    {
        var users = await _userService.GetUsersAsync(query, cancellationToken);
        var userListResponse = new QueryablePaging<UserResponse>(users.Count, users.Query.ProjectToType<UserResponse>());

        return Ok(userListResponse);
    }
}

using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.Accounts.Naics;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.NaicsCodes.Responses;
using TaxBeacon.API.Exceptions;

namespace TaxBeacon.API.Controllers.NaicsCodes;
[Authorize]
public class NaicsController: BaseController
{
    private readonly INaicsService _naicsService;
    public NaicsController(INaicsService naicsService) => _naicsService = naicsService;

    /// <summary>
    /// Get NAICS codes
    /// </summary>
    /// <response code="200">Returns NAICS codes</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>A collection of NAICS codes</returns>
    [HasPermissions(Common.Permissions.Accounts.Read, Common.Permissions.Accounts.ReadWrite)]
    [HttpGet(Name = "GetNaicsCodes")]
    [ProducesDefaultResponseType(typeof(CustomProblemDetails))]
    [ProducesResponseType(typeof(IEnumerable<NaicsCodeTreeItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ResponseCache(Duration = 60 * 60 * 30, Location = ResponseCacheLocation.Any, NoStore = false)]
    public async Task<IActionResult> GetNaicsCodesAsync(CancellationToken cancellationToken)
    {
        var codes = await _naicsService.GetNaicsCodesAsync(cancellationToken);

        return Ok(codes.Adapt<List<NaicsCodeTreeItemResponse>>());
    }
}

using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaxBeacon.API.Controllers.GlobalSearch.Requests;
using TaxBeacon.API.Controllers.GlobalSearch.Responses;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.Controllers.GlobalSearch;

[Authorize]
public class SearchController: BaseController
{
    private readonly IGlobalSearchService _service;

    public SearchController(IGlobalSearchService service) => _service = service;

    /// <summary>
    /// Endpoint to update user information
    /// </summary>
    /// <param name="searchRequest">Search request containing search term</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Collection of search results</response>
    /// <response code="401">User is unauthorized</response>
    /// <returns>User details</returns>
    [HttpPost(Name = "GlobalSearch")]
    [ProducesResponseType(typeof(IEnumerable<SearchResultsResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchAsync([FromBody] SearchRequest searchRequest, CancellationToken cancellationToken)
    {
        var resultsDto = await _service.SearchAsync(
            searchRequest.Text,
            searchRequest.Page,
            searchRequest.PageSize,
            searchRequest.LastUpdatedDateTime,
            cancellationToken);

        return Ok(resultsDto.Adapt<SearchResultsResponse>());
    }
}

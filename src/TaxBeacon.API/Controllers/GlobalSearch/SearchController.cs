using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using TaxBeacon.API.Controllers.GlobalSearch.Responses;
using TaxBeacon.Common.Services;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.API.Controllers.GlobalSearch;

[Authorize]
public class SearchController: BaseController
{
    private readonly SearchClient _client;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;

    public SearchController(SearchClient client, ICurrentUserService currentUserService, IUserService userService)
    {
        _client = client;
        _currentUserService = currentUserService;
        _userService = userService;
    }

    /// <summary>
    /// Endpoint to update user information
    /// </summary>
    /// <param name="searchText">Search Text</param>
    /// <param name="cancellationToken"></param>
    /// <response code="200">Collection of search results</response>
    /// <response code="401">User is unauthorized</response>
    /// <returns>User details</returns>
    [HttpPost(Name = "GlobalSearch")]
    [ProducesResponseType(typeof(IEnumerable<SearchResultResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Search([FromBody] string searchText, CancellationToken cancellationToken)
    {
        var options = new SearchOptions
        {
            SearchMode = SearchMode.All,
            QueryType = SearchQueryType.Full,
            Size = 5,
            OrderBy = { "LastModifiedDateTimeUtc desc", "CreatedDateTimeUtc" },
            Filter = await CreateFilterExpressionAsync()
        };

        var response = await _client.SearchAsync<SearchResult>($"/.*{searchText}.*/", options, cancellationToken);

        var results = response.Value
            .GetResults()
            .Select(result => result.Document)
            .ToList();

        return Ok(results.Adapt<SearchResultResponse[]>());
    }

    private async Task<string> CreateFilterExpressionAsync()
    {
        var filterExpressions = new List<string>();

        var userPermissions = await _userService.GetUserPermissionsAsync(_currentUserService.UserId);
        filterExpressions.Add($"Permissions/any(p:search.in(p, '{string.Join(',', userPermissions)}'))");

        filterExpressions.Add(_currentUserService is { IsSuperAdmin: true, IsUserInTenant: false }
            ? $"TenantId eq null"
            : $"TenantId eq '{_currentUserService.TenantId}'");

        if (!_currentUserService.DivisionEnabled)
        {
            filterExpressions.Add($"EntityType ne 'division'");
        }

        return string.Join(" and ", filterExpressions);
    }
}

public class SearchResult
{
    [JsonPropertyName("OriginalId")]
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? LastModifiedDateTimeUtc { get; set; }
}
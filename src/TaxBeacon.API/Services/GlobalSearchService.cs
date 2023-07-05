using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using TaxBeacon.Administration.Users;
using TaxBeacon.Common.Models;
using TaxBeacon.Common.Services;

namespace TaxBeacon.API.Services;

public class GlobalSearchService: IGlobalSearchService
{
    private readonly SearchClient _client;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUserService _userService;
    private readonly ILogger<GlobalSearchService> _logger;

    public GlobalSearchService(SearchClient client, ICurrentUserService currentUserService, IUserService userService, ILogger<GlobalSearchService> logger)
    {
        _client = client;
        _currentUserService = currentUserService;
        _userService = userService;
        _logger = logger;
    }

    public async Task<SearchResultsDto> SearchAsync(string term, int page, int pageSize, DateTime? filterDateTime,
        CancellationToken cancellationToken = default)
    {
        var options = new SearchOptions
        {
            SearchMode = SearchMode.All,
            QueryType = SearchQueryType.Full,
            Size = pageSize,
            Skip = pageSize * (page - 1),
            OrderBy = { "LastModifiedDateTimeUtc desc", "CreatedDateTimeUtc" },
            Filter = await CreateFilterExpressionAsync(filterDateTime),
            IncludeTotalCount = true,
            HighlightPreTag = "<b>",
            HighlightPostTag = "</b>"
        };

        var highlightFields = SearchIndex.GetHighlightFields();
        foreach (var field in highlightFields)
        {
            options.HighlightFields.Add(field);
        }

        var searchWords = term.Split(' ').Select(word => $"/.*{Regex.Escape(word)}.*/").ToList();
        try
        {
            var response =
                await _client.SearchAsync<SearchResultItemDto>(string.Join(" AND ", searchWords), options,
                    cancellationToken);

            var items = response.Value
                .GetResults()
                .Select(result =>
                {
                    var item = result.Document;
                    item.Highlights = result.Highlights?
                        .Select(kv => new Highlight { Field = kv.Key, Values = kv.Value.ToArray() }).ToArray();

                    return item;
                })
                .ToArray();

            var totalCount = response.Value.TotalCount;

            return new SearchResultsDto { Count = totalCount!.Value, Items = items };
        }
        catch (RequestFailedException exception)
        {
            _logger.LogError(exception, "Azure Cognitive Search exception");
            return new SearchResultsDto { Count = 0, Items = { } };
        }
    }

    private async Task<string> CreateFilterExpressionAsync(DateTime? filterDateTime)
    {
        var filterExpressions = new List<string>();

        var userPermissions = await _userService.GetUserPermissionsAsync(_currentUserService.UserId, _currentUserService.TenantId);
        filterExpressions.Add($"Permissions/any(p:search.in(p, '{string.Join(',', userPermissions)}'))");

        filterExpressions.Add(_currentUserService is { IsSuperAdmin: true, IsUserInTenant: false }
            ? $"TenantId eq null"
            : $"TenantId eq '{_currentUserService.TenantId}'");

        if (!_currentUserService.DivisionEnabled)
        {
            filterExpressions.Add($"EntityType ne 'division'");
        }

        if (filterDateTime is not null)
        {
            filterExpressions.Add($"LastModifiedDateTimeUtc ge {filterDateTime.Value:o}");
        }

        return string.Join(" and ", filterExpressions);
    }
}

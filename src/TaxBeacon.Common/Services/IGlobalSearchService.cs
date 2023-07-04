using TaxBeacon.Common.Models;

namespace TaxBeacon.Common.Services;

public interface IGlobalSearchService
{
    Task<SearchResultsDto> SearchAsync(string term, int page, int pageSize,
        CancellationToken cancellationToken = default);
}

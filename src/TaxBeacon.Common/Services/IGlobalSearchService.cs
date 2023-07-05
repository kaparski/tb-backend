using TaxBeacon.Common.Models;

namespace TaxBeacon.Common.Services;

public interface IGlobalSearchService
{
    Task<SearchResultsDto> SearchAsync(string term, int page, int pageSize, DateTime? filterDateTime = null,
        CancellationToken cancellationToken = default);
}

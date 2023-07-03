using Mapster;

namespace TaxBeacon.API.Controllers.GlobalSearch.Responses;

public record SearchResultsResponse(long Count, SearchResultItemResponse[] Items);

public record SearchResultItemResponse
{
    public Guid Id { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public string EntityType { get; init; } = string.Empty;

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public HighlightResponse[]? Highlights { get; init; }
}

public record HighlightResponse(string Field, string[] Values);
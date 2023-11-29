using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.GlobalSearch.Responses;

public record SearchResultsResponse(long Count, long PagesCount, SearchResultItemResponse[] Items);

public record SearchResultItemResponse
{
    public Guid Id { get; init; }

    public Guid? AdditionalId { get; init; }

    public string DisplayName { get; init; } = string.Empty;

    public SearchEntityType EntityType { get; init; } = null!;

    public DateTime CreatedDateTimeUtc { get; init; }

    public DateTime? LastModifiedDateTimeUtc { get; init; }

    public HighlightResponse[]? Highlights { get; init; }
}

public record HighlightResponse(string Field, string[] Values);

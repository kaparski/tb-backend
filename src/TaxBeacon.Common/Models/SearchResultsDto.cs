using System.Text.Json.Serialization;

namespace TaxBeacon.Common.Models;

public class SearchResultsDto
{
    public long Count { get; set; }

    public SearchResultItemDto[] Items { get; set; } = { };
}

public class SearchResultItemDto
{
    [JsonPropertyName("OriginalId")]
    public Guid Id { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    public string EntityType { get; set; } = string.Empty;

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? LastModifiedDateTimeUtc { get; set; }

    public Highlight[]? Highlights { get; set; } = { };
}
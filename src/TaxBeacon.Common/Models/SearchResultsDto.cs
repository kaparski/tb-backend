using Ardalis.SmartEnum.SystemTextJson;
using System.Text.Json.Serialization;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Common.Models;

public class SearchResultsDto
{
    public long Count { get; set; } = 0;

    public long PagesCount { get; set; } = 0;

    public SearchResultItemDto[] Items { get; set; } = { };
}

public class SearchResultItemDto
{
    [JsonPropertyName("OriginalId")]
    public Guid Id { get; set; }

    public Guid? AdditionalId { get; set; }

    public string DisplayName { get; set; } = string.Empty;

    [JsonConverter(typeof(SmartEnumNameConverter<SearchEntityType, int>))]
    public SearchEntityType? EntityType { get; set; } = null!;

    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? LastModifiedDateTimeUtc { get; set; }

    public Highlight[]? Highlights { get; set; } = { };
}
using Azure.Search.Documents.Indexes;

namespace TaxBeacon.Common.Models;

public class SearchIndex
{
    [SimpleField(IsKey = true)]
    public string DocumentId { get; set; } = null!;

    [SimpleField]
    public string OriginalId { get; set; } = null!;

    [SimpleField(IsFilterable = true)]
    public string TenantId { get; set; } = null!;

    [SimpleField]
    public string DisplayName { get; set; } = null!;

    [SearchableField]
    public string? Name { get; set; }

    [SearchableField]
    public string? Description { get; set; }

    [SearchableField]
    public string? LegalName { get; set; }

    [SearchableField]
    public string? FullName { get; set; }

    [SearchableField]
    public string? Email { get; set; }

    [SearchableField]
    public string? Reference { get; set; }

    [SearchableField]
    public string? Overview { get; set; }

    [SearchableField]
    public string? LegalAuthority { get; set; }

    [SearchableField]
    public string? Agency { get; set; }

    [SearchableField]
    public string? JurisdictionName { get; set; }

    [SearchableField]
    public string? State { get; set; }

    [SearchableField]
    public string? County { get; set; }

    [SearchableField]
    public string? City { get; set; }

    [SearchableField]
    public string? IncentivesArea { get; set; }

    [SearchableField]
    public string? IncentivesType { get; set; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTime CreatedDateTimeUtc { get; set; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTime? LastModifiedDateTimeUtc { get; set; }

    [SimpleField(IsFilterable = true)]
    public string EntityType { get; set; } = null!;

    [SimpleField(IsFilterable = true, IsHidden = true)]
    public string[] Permissions { get; set; } = { };

    public static IEnumerable<string> GetHighlightFields()
    {
        var properties = typeof(SearchIndex).GetProperties();

        return properties
            .Where(prop => prop.GetCustomAttributes(typeof(SearchableFieldAttribute), true).Any())
            .Select(prop => prop.Name)
            .ToArray();
    }
}

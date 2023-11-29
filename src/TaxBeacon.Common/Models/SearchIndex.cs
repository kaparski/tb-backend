using Azure.Search.Documents.Indexes;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.Common.Models;

public class SearchIndex
{
    [SimpleField(IsKey = true)]
    public string DocumentId { get; set; } = null!;

    [SimpleField]
    public string OriginalId { get; set; } = null!;

    [SimpleField(IsFilterable = true)]
    public string TenantId { get; set; } = null!;

    [SimpleField(IsFilterable = true)]
    public string AdditionalId { get; set; } = null!;

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
    public string? SecondaryEmail { get; set; }

    [SearchableField]
    public string? JobTitle { get; set; }

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
    public string? Country { get; set; }

    [SearchableField]
    public string? State { get; set; }

    [SearchableField]
    public string? County { get; set; }

    [SearchableField]
    public string? City { get; set; }

    [SearchableField]
    public string? Address1 { get; set; }

    [SearchableField]
    public string? Address2 { get; set; }

    [SearchableField]
    public string? Zip { get; set; }

    [SearchableField]
    public string? Address { get; set; }

    [SearchableField]
    public string? IncentivesArea { get; set; }

    [SearchableField]
    public string? IncentivesType { get; set; }

    [SearchableField]
    public string? DoingBusinessAs { get; set; }

    [SearchableField]
    public string? LinkedInUrl { get; set; }

    [SearchableField]
    public string? Website { get; set; }

    [SearchableField]
    public string? NaicsCode { get; set; }

    [SearchableField]
    public string? EmployeeCount { get; set; }

    [SearchableField]
    public string? AnnualRevenue { get; set; }

    [SearchableField]
    public string? FoundationYear { get; set; }

    [SearchableField]
    public string? Fein { get; set; }

    [SearchableField]
    public string? Ein { get; set; }

    [SearchableField]
    public string? AccountId { get; set; }

    [SearchableField]
    public string? LocationId { get; set; }

    [SearchableField]
    public string? EntityId { get; set; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTime CreatedDateTimeUtc { get; set; }

    [SimpleField(IsFilterable = true, IsSortable = true)]
    public DateTime? LastModifiedDateTimeUtc { get; set; }

    [SimpleField(IsFilterable = true)]
    public SearchEntityType EntityType { get; set; } = null!;

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

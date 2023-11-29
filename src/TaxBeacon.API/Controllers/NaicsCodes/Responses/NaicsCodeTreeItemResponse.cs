using System.Collections.ObjectModel;

namespace TaxBeacon.API.Controllers.NaicsCodes.Responses;

public record NaicsCodeTreeItemResponse
{
    public int Code { get; init; }

    public string Title { get; init; } = null!;

    public string? Description { get; init; }

    public int? ParentCode { get; init; }

    public ICollection<NaicsCodeTreeItemResponse> Children { get; set; } = new Collection<NaicsCodeTreeItemResponse>();
}

using Mapster;
using System.Collections.ObjectModel;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.Accounts.Naics.Models;

public record NaicsCodeTreeItemDto
{
    public int Code { get; init; }

    public string Title { get; init; } = null!;

    public string? Description { get; init; }

    public int? ParentCode { get; init; }

    public ICollection<NaicsCodeTreeItemDto> Children { get; set; } = new Collection<NaicsCodeTreeItemDto>();
}


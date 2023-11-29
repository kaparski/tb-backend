using System.Collections.ObjectModel;

namespace TaxBeacon.API.Shared.Responses;

public record NaicsCodeResponse
{
    public int Code { get; init; }

    public string Title { get; init; } = null!;
}

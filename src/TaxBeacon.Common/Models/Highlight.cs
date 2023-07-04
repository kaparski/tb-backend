namespace TaxBeacon.Common.Models;

public record Highlight
{
    public string Field { get; init; } = null!;

    public string[] Values { get; init; } = { };
}

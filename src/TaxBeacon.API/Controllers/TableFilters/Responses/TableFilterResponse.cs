namespace TaxBeacon.API.Controllers.TableFilters.Responses;

public class TableFilterResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Configuration { get; set; } = null!;
}

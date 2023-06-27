namespace TaxBeacon.UserManagement.TableFilters.Models;

public class TableFilterDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Configuration { get; set; } = null!;
}

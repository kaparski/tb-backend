using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.TableFilters.Models;

public class CreateTableFilterDto
{
    public string Name { get; init; } = null!;

    public string Configuration { get; init; } = null!;

    public EntityType TableType { get; init; }
}

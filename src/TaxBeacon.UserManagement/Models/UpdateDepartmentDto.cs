namespace TaxBeacon.UserManagement.Models;

public record UpdateDepartmentDto(
    string Name, string Description,
    Guid? DivisionId,
    IEnumerable<Guid>? ServiceAreasIds,
    IEnumerable<Guid>? JobTitlesIds
    );


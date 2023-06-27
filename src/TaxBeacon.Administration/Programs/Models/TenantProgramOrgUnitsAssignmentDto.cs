namespace TaxBeacon.Administration.Programs.Models;

public record TenantProgramOrgUnitsAssignmentDto(
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? ServiceAreaId,
    string? ServiceAreaName);


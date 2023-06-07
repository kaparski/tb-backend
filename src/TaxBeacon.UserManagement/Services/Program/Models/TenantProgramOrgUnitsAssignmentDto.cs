namespace TaxBeacon.UserManagement.Services.Program.Models;

public record TenantProgramOrgUnitsAssignmentDto(
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? ServiceAreaId,
    string? ServiceAreaName);


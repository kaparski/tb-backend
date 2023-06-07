namespace TaxBeacon.API.Controllers.Programs.Responses;

public record TenantProgramOrgUnitsAssignmentResponse(
    Guid? DepartmentId,
    string? DepartmentName,
    Guid? ServiceAreaId,
    string? ServiceAreaName);

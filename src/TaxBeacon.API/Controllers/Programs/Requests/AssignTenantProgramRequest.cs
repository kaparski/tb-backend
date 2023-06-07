namespace TaxBeacon.API.Controllers.Programs.Requests;

public record AssignTenantProgramRequest(Guid? DepartmentId, Guid? ServiceAreaId);

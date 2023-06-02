namespace TaxBeacon.API.Controllers.Tenants.Requests;

public record ChangeTenantProgramsRequest(IEnumerable<Guid> ProgramsIds);

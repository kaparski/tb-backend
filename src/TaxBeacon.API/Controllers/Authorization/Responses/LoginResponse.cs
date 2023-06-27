
namespace TaxBeacon.API.Controllers.Authorization.Responses;

public record LoginResponse(Guid UserId, string FullName, IReadOnlyCollection<string> Permissions, bool IsSuperAdmin, bool? DivisionsEnabled);
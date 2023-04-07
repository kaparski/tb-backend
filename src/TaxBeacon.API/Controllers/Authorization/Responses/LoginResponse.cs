
namespace TaxBeacon.API.Controllers.Authorization.Responses
{
    public record LoginResponse(Guid UerId, string FullName, IReadOnlyCollection<string> Permissions);
}

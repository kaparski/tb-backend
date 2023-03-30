
namespace TaxBeacon.API.Controllers.Authorization.Responses
{
    public sealed class LoginResponse
    {
        public Guid UserId { get; set; }

        public string FullName { get; set; } = null!;

        public IReadOnlyCollection<string> Permissions { get; set; } = Array.Empty<string>();

    }
}

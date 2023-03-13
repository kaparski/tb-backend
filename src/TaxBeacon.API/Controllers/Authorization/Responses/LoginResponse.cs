namespace TaxBeacon.API.Controllers.Authorization.Responses
{
    public sealed class LoginResponse
    {
        public IReadOnlyCollection<string> Permissions { get; set; } = Array.Empty<string>();
    }
}

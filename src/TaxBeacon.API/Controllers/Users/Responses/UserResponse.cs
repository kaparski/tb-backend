using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Users.Responses;

public class UserResponse
{
    public Guid Id { get; set; }

    public string Username { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public UserStatus UserStatus { get; set; }

    public DateTime? LastLoginDateUtc { get; set; }

    public string Email { get; set; }

    public string Role { get; set; }

    public string Department { get; set; }

    public string JobTitle { get; set; }
}

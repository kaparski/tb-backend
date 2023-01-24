using TaxBeacon.Common.Enums;

namespace TaxBeacon.UserManagement.Models;

public class UserList
{
    public Guid Id { get; set; }

    public string Username { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public UserStatus UserStatus { get; set; }

    public DateTime? LastLoginDateUtc { get; set; }
}

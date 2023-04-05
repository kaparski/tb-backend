namespace TaxBeacon.UserManagement.Models;

public sealed class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public override string ToString() => $"First Name to {FirstName}, Last Name to {LastName}";
}

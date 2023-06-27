namespace TaxBeacon.UserManagement.Users.Models;

public sealed class UpdateUserDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LegalName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public Guid? DivisionId { get; set; }

    public Guid? DepartmentId { get; set; }

    public Guid? ServiceAreaId { get; set; }

    public Guid? JobTitleId { get; set; }

    public Guid? TeamId { get; set; }
}

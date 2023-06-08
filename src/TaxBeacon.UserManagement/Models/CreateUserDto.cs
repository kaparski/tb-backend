namespace TaxBeacon.UserManagement.Models;

public record CreateUserDto(
    string FirstName,
    string LegalName,
    string LastName,
    string Email,
    Guid? DivisionId,
    Guid? DepartmentId,
    Guid? ServiceAreaId,
    Guid? JobTitleId,
    Guid? TeamId);

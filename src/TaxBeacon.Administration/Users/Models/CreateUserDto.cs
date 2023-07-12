namespace TaxBeacon.Administration.Users.Models;

public record CreateUserDto(
    string FirstName,
    string LegalName,
    string LastName,
    string Email,
    string? ExternalAadUserObjectId,
    Guid? DivisionId,
    Guid? DepartmentId,
    Guid? ServiceAreaId,
    Guid? JobTitleId,
    Guid? TeamId);

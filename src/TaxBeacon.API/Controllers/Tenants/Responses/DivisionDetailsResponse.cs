namespace TaxBeacon.API.Controllers.Tenants.Responses;

public class DivisionDetailsResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; }

    public int NumberOfUsers { get; set; }

    public string Description { get; set; }

    public DateTime CreatedDateTimeUtc { get; set; }

    public IEnumerable<DepartmentResponse> Departments { get; set; } = Enumerable.Empty<DepartmentResponse>();

    public record DepartmentResponse(string Name);
}

namespace TaxBeacon.UserManagement.Models
{
    public sealed class UpdateDivisionDto
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public IEnumerable<Guid>? DepartmentIds { get; set; }
    }
}

namespace TaxBeacon.UserManagement.Models
{
    public record PermissionDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; }
        public string Category { get; set; }
    }
}

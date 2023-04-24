namespace TaxBeacon.UserManagement.Models
{
    public class DivisionDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedDateTimeUtc { get; set; }

        public int NumberOfUsers { get; set; }

        public string Departments { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;
    }
}

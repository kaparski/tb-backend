namespace TaxBeacon.UserManagement.Models
{
    public class DivisionDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime CreatedDateTimeUtc { get; set; }

        public int NumberOfUsers { get; set; }

        public string Departments { get; set; } = string.Empty;
    }
}

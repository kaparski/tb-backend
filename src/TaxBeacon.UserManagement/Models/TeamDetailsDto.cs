namespace TaxBeacon.UserManagement.Models
{
    public class TeamDetailsDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;

        public string Description { get; set; } = null!;

        public DateTime CreatedDateTimeUtc { get; set; }
    }
}

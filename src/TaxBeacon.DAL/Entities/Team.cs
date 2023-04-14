namespace TaxBeacon.DAL.Entities
{
    public class Team: BaseEntity
    {
        public Guid TenantId { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Tenant Tenant { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new HashSet<User>();

    }
}

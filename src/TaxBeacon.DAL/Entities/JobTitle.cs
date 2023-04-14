namespace TaxBeacon.DAL.Entities
{
    public class JobTitle: BaseEntity
    {
        public Guid TenantId { get; set; }

        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public Guid DepartmentId { get; set; }

        public Department Department { get; set; } = null!;

        public ICollection<User> Users { get; set; } = new HashSet<User>();

    }
}

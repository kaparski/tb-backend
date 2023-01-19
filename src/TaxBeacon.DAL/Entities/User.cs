namespace TaxBeacon.DAL.Entities;

public class User: BaseEntity
{
    public Guid Id { get; set; }

    public string Username { get; set; }

    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string Email { get; set; }

    public int StatusId { get; set; }

    //TODO: Add department, roles and Job title in the future

    public ICollection<TenantUser> TenantUsers { get; set; } = new HashSet<TenantUser>();
}

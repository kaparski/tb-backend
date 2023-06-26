namespace TaxBeacon.DAL.Entities.Accounts;
public class ClientManager
{
    public Guid ManagerId { get; set; }

    public Guid AccountId { get; set; }

    public Guid TenantId { get; set; }

    public User Manager { get; set; } = null!;

    public Client Client { get; set; } = null!;
}

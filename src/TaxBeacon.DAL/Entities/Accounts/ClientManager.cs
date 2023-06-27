namespace TaxBeacon.DAL.Entities.Accounts;
public class ClientManager
{
    public Guid UserId { get; set; }

    public Guid AccountId { get; set; }

    public Guid TenantId { get; set; }

    public TenantUser User { get; set; } = null!;

    public Client Client { get; set; } = null!;
}

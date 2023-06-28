using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;
public class ClientManager
{
    public Guid UserId { get; set; }

    public Guid AccountId { get; set; }

    public Guid TenantId { get; set; }

    public TenantUser TenantUser { get; set; } = null!;

    public Client Client { get; set; } = null!;
}

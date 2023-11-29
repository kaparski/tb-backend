using TaxBeacon.Common.Enums;
using TaxBeacon.DAL.Administration.Entities;

namespace TaxBeacon.DAL.Accounts.Entities;

public class Contact: BaseDeletableEntity
{
    public Guid Id { get; set; }

    public Guid TenantId { get; set; }

    public string Email { get; set; } = null!;

    public string? SecondaryEmail { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? JobTitle { get; set; }

    public string FullName { get; private set; } = string.Empty;

    public Tenant Tenant { get; set; } = null!;

    public ICollection<AccountContact> Accounts { get; set; } = new HashSet<AccountContact>();

    public ICollection<Client> Clients { get; set; } = new HashSet<Client>();

    public ICollection<Referral> Referrals { get; set; } = new HashSet<Referral>();

    public ICollection<ContactActivityLog> ContactActivityLogs { get; set; } = new HashSet<ContactActivityLog>();

    public ICollection<ContactPhone> Phones { get; set; } = new HashSet<ContactPhone>();

    public ICollection<LinkedContact> LinkedContacts { get; set; } = new HashSet<LinkedContact>();
}

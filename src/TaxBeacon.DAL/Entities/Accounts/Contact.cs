using System.Net.Mail;
using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.DAL.Entities.Accounts;
public class Contact: BaseEntity
{
    public Guid TenantId { get; set; }

    public Tenant Tenant { get; set; } = null!;

    public Guid AccountId { get; set; }

    public Account Account { get; set; } = null!;

    public Guid Id { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public MailAddress Email { get; set; } = null!;

    public string? JobTitle { get; set; }

    public ContactType Type { get; set; } = ContactType.None;

    public string? Phone { get; set; }

    public Status Status { get; set; }

    public string FullName { get; private set; } = string.Empty;

}

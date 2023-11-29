using TaxBeacon.DAL.Accounts.Common;

namespace TaxBeacon.DAL.Accounts.Entities;

public class ContactPhone: PhoneBase
{
    public Guid ContactId { get; set; }

    public Contact Contact { get; set; } = null!;
}

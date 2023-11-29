using TaxBeacon.DAL.Accounts.Common;

namespace TaxBeacon.DAL.Accounts.Entities;

public class AccountPhone: PhoneBase
{
    public Guid? AccountId { get; set; }

    public Account? Account { get; set; }
}

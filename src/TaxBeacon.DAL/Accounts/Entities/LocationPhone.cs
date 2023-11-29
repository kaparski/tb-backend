using TaxBeacon.DAL.Accounts.Common;

namespace TaxBeacon.DAL.Accounts.Entities;

public class LocationPhone: PhoneBase
{
    public Guid LocationId { get; set; }

    public Location Location { get; set; } = null!;
}

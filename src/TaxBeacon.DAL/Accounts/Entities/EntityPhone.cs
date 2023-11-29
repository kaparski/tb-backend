using TaxBeacon.DAL.Accounts.Common;

namespace TaxBeacon.DAL.Accounts.Entities;

public class EntityPhone: PhoneBase
{
    public Guid? EntityId { get; set; }

    public Entity? Entity { get; set; }
}

namespace TaxBeacon.DAL.Accounts.Entities;
public class StateId
{
    public Guid Id { get; set; }

    public int Value { get; set; }
    public Guid EntityId { get; set; }

    public Entity Entity { get; set; } = null!;
}

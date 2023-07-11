namespace TaxBeacon.DAL.Accounts.Entities;

public class Phone
{
    public Guid Id { get; set; }

    public Guid? AccountId { get; set; }

    public string Type { get; set; } = null!;

    public string Number { get; set; } = null!;

    public string? Extension { get; set; }

    public Account? Account { get; set; }
}

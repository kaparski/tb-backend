using Ardalis.SmartEnum;
using System.Diagnostics.CodeAnalysis;

namespace TaxBeacon.Common.Enums.Accounts;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class AccountEntityType: SmartEnum<AccountEntityType>
{
    public static readonly AccountEntityType None = new("None", 0);
    public static readonly AccountEntityType CCorp = new("C-Corp", 1);
    public static readonly AccountEntityType GP = new("GP", 2);
    public static readonly AccountEntityType LLC = new("LLC", 3);
    public static readonly AccountEntityType LLP = new("LLP", 4);
    public static readonly AccountEntityType LP = new("LP", 5);
    public static readonly AccountEntityType SCorp = new("S-Corp", 6);
    public static readonly AccountEntityType SoleProprietorship = new("Sole Proprietorship", 7);
    public static readonly AccountEntityType Type501cNonprofit = new("501c Nonprofit", 8);
    public static readonly AccountEntityType Unknown = new("Unknown", 9);
    public static readonly AccountEntityType Other = new("Other", 10);

    private AccountEntityType(string name, int value) : base(name, value)
    {
    }
}

using Ardalis.SmartEnum;
using System.Diagnostics.CodeAnalysis;

namespace TaxBeacon.Common.Accounts;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class EntityType: SmartEnum<EntityType>
{
    public static readonly EntityType None = new("None", 0);
    public static readonly EntityType CCorp = new("C-Corp", 1);
    public static readonly EntityType GP = new("GP", 2);
    public static readonly EntityType LLC = new("LLC", 2);
    public static readonly EntityType LLP = new("LLP", 3);
    public static readonly EntityType SCorp = new("S-Corp", 4);
    public static readonly EntityType SoleProprietorship = new("Sole Proprietorship", 5);
    public static readonly EntityType Type501cNonprofit = new("501c Nonprofit", 6);
    public static readonly EntityType Unknown = new("Unknown", 7);
    public static readonly EntityType Other = new("Other", 8);

    private EntityType(string name, int value) : base(name, value)
    {
    }
}

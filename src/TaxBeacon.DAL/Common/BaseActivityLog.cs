namespace TaxBeacon.DAL.Common;

public class BaseActivityLog<T> where T : Enum
{
    public DateTime Date { get; set; }

    public T? EventType { get; set; }

    public uint Revision { get; set; }

    public string Event { get; set; } = null!;
}

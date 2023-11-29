namespace TaxBeacon.DAL.Common;

public abstract class BaseEntity
{
    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? LastModifiedDateTimeUtc { get; set; }
}

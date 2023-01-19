namespace TaxBeacon.DAL.Common;

public abstract class BaseEntity
{
    public DateTime CreatedDateUtc { get; set; }

    public DateTime? LastModifiedUtc { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedDateUtc { get; set; }
}

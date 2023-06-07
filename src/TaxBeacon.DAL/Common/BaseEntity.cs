namespace TaxBeacon.DAL.Common;

public abstract class BaseEntity: IDeletableEntity
{
    public DateTime CreatedDateTimeUtc { get; set; }

    public DateTime? LastModifiedDateTimeUtc { get; set; }

    public bool? IsDeleted { get; set; }

    public DateTime? DeletedDateTimeUtc { get; set; }
}

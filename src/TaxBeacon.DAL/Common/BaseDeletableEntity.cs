namespace TaxBeacon.DAL.Common;

public abstract class BaseDeletableEntity: BaseEntity, IDeletableEntity
{
    public bool? IsDeleted { get; set; }

    public DateTime? DeletedDateTimeUtc { get; set; }
}

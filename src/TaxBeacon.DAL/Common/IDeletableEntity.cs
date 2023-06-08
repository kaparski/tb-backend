namespace TaxBeacon.DAL.Common;

public interface IDeletableEntity
{
    public bool? IsDeleted { get; set; }

    public DateTime? DeletedDateTimeUtc { get; set; }
}

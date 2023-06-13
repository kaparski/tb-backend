namespace TaxBeacon.Accounts.Entities;
public interface IEntityService
{
    IQueryable<EntityDto> QueryEntities();
}

namespace TaxBeacon.Common.Enums.Administration.Activities;

public enum EntityEventType
{
    None = 0,
    EntityUpdated = 1,
    EntityDeactivated = 2,
    EntityReactivated = 3,
    EntityCreated = 4,
    EntityStateIdDeleted = 5,
    EntityStateIdAdded = 6,
    EntityStateIdUpdated = 7,
    LocationUnassociated = 8,
    LocationAssociated = 9,
    EntityImported = 10,
}

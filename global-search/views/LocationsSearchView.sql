CREATE OR ALTER VIEW LocationsSearchView AS
  SELECT
    CONCAT_WS('_', L.Id, L.TenantId) AS DocumentId,
    Id AS OriginalId,
    TenantId as TenantId,
    AccountId as AdditionalId,
    Name,
    LocationId,
    Country,
    State,
    County,
    City,
    Address1,
    Address2,
    Zip,
    Address,
    PrimaryNaicsCode as NaicsCode,
    CreatedDateTimeUtc,
    LastModifiedDateTimeUtc,
    IsDeleted,
    JSON_ARRAY(
      'Locations.Read',
      'Locations.ReadWrite',
      'Locations.ReadExport',
      'Locations.ReadActivation') as Permissions,
    'location' as EntityType
  FROM Locations L

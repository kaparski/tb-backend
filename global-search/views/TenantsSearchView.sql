CREATE OR ALTER VIEW TenantsSearchView AS
    SELECT
        Id AS DocumentId,
        Id AS OriginalId,
        Name,
        CreatedDateTimeUtc,
        LastModifiedDateTimeUtc,
        IsDeleted,
        JSON_ARRAY('Tenants.Read', 'Tenants.ReadWrite', 'Tenants.ReadExport') as Permissions,
        'tenant' as EntityType
    FROM Tenants
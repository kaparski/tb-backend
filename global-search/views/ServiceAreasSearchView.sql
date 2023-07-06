CREATE OR ALTER VIEW ServiceAreasSearchView AS
    SELECT
        CONCAT_WS('_', Id, TenantId) AS DocumentId,
        Id AS OriginalId,
        TenantId,
        Name,
        Description,
        CreatedDateTimeUtc,
        LastModifiedDateTimeUtc,
        IsDeleted,
        JSON_ARRAY('ServiceAreas.Read', 'ServiceAreas.ReadWrite', 'ServiceAreas.ReadExport') as Permissions,
        'service area' as EntityType
    FROM ServiceAreas
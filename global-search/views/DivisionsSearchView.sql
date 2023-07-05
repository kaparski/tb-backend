CREATE OR ALTER VIEW DivisionsSearchView AS
    SELECT
        CONCAT_WS('_', Id, TenantId) AS DocumentId,
        Id AS OriginalId,
        TenantId,
        Name,
        Description,
        CreatedDateTimeUtc,
        LastModifiedDateTimeUtc,
        IsDeleted,
        JSON_ARRAY('Divisions.Read', 'Divisions.ReadWrite', 'Divisions.ReadExport') as Permissions,
        'division' as EntityType
    FROM Divisions
CREATE OR ALTER VIEW TeamsSearchView AS
    SELECT
        CONCAT_WS('_', Id, TenantId) AS DocumentId,
        Id AS OriginalId,
        TenantId,
        Name,
        Description,
        CreatedDateTimeUtc,
        LastModifiedDateTimeUtc,
        IsDeleted,
        JSON_ARRAY('Teams.Read', 'Teams.ReadWrite', 'Teams.ReadExport') as Permissions,
        'team' as EntityType
    FROM Teams
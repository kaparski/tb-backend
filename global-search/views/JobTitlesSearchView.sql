CREATE OR ALTER VIEW JobTitlesSearchView AS
    SELECT
        CONCAT_WS('_', Id, TenantId) AS DocumentId,
        Id AS OriginalId,
        TenantId,
        Name,
        Description,
        CreatedDateTimeUtc,
        LastModifiedDateTimeUtc,
        IsDeleted,
        JSON_ARRAY('JobTitles.Read', 'JobTitles.ReadWrite', 'JobTitles.ReadExport') as Permissions,
        'job title' as EntityType
    FROM JobTitles
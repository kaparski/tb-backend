CREATE OR ALTER VIEW DepartmentsSearchView AS
    SELECT
        CONCAT_WS('_', Id, TenantId) AS DocumentId,
        Id AS OriginalId,
        TenantId,
        Name,
        Description,
        CreatedDateTimeUtc,
        LastModifiedDateTimeUtc,
        IsDeleted,
        JSON_ARRAY('Departments.Read', 'Departments.ReadWrite', 'Departments.ReadExport') as Permissions,
        'department' as EntityType
    FROM Departments
CREATE OR ALTER VIEW RolesSearchView AS
    SELECT
        CONCAT_WS('_', R.Id, TR.TenantId) AS DocumentId,
        R.Id AS OriginalId,
        TR.TenantId AS TenantId,
        R.Name,
        R.CreatedDateTimeUtc,
        R.LastModifiedDateTimeUtc,
        R.IsDeleted,
        JSON_ARRAY('Roles.Read', 'Roles.ReadWrite', 'Roles.ReadExport') as Permissions,
        'role' as EntityType
    FROM Roles R
        LEFT JOIN TenantRoles TR on R.Id = TR.RoleId
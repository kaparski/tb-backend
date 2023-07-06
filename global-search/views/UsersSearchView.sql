CREATE OR ALTER VIEW UsersSearchView AS
    SELECT
        CONCAT_WS('_', U.Id, TU.TenantId) AS DocumentId,
        U.Id AS OriginalId,
        TU.TenantId AS TenantId,
        FullName,
        LegalName,
        Email,
        IsDeleted,
        CreatedDateTimeUtc,
        LastModifiedDateTimeUtc,
        JSON_ARRAY('Users.Read', 'Users.ReadWrite', 'Users.ReadExport') as Permissions,
        'user' as EntityType
    FROM Users U
        LEFT JOIN TenantUsers TU on U.Id = TU.UserId
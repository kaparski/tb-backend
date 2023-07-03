CREATE OR ALTER VIEW ProgramsSearchView AS
    WITH ProgramsCTE
    (
        OriginalId,
        Name,
        Reference,
        Overview,
        LegalAuthority,
        Agency,
        JurisdictionName,
        State,
        County,
        City,
        IncentivesArea,
        IncentivesType,
        IsDeleted,
        CreatedDateTimeUtc,
        LastModifiedDateTimeUtc,
        Permissions,
        EntityType
    )
    AS
    (
        SELECT
            Id AS OriginalId,
            Name,
            Reference,
            Overview,
            LegalAuthority,
            Agency,
            JurisdictionName,
            State,
            County,
            City,
            IncentivesArea,
            IncentivesType,
            IsDeleted,
            CreatedDateTimeUtc,
            LastModifiedDateTimeUtc,
            JSON_ARRAY('Programs.Read', 'Programs.ReadWrite', 'Programs.ReadExport') as Permissions,
            'program' as EntityType
        FROM Programs
    )

    SELECT
        CONVERT(nvarchar(36), OriginalId) AS DocumentId,
        NULL AS TenantId,
        P.*
    FROM ProgramsCTE P
    UNION
    SELECT
        CONCAT_WS('_', P.OriginalId, TP.TenantId) AS DocumentId,
        TP.TenantId AS TenantId,
        P.OriginalId,
        P.Name,
        P.Reference,
        P.Overview,
        P.LegalAuthority,
        P.Agency,
        P.JurisdictionName,
        P.State,
        P.County,
        P.City,
        P.IncentivesArea,
        P.IncentivesType,
        TP.IsDeleted AS IsDeleted,
        P.CreatedDateTimeUtc,
        P.LastModifiedDateTimeUtc,
        P.Permissions,
        P.EntityType
    FROM ProgramsCTE P
         INNER JOIN TenantsPrograms TP on P.OriginalId = TP.ProgramId
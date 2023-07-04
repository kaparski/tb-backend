CREATE OR ALTER VIEW AccountsSearchView AS
    SELECT
        CONCAT_WS('_', A.Id, A.TenantId) AS DocumentId,
        A.Id AS OriginalId,
        A.Name,
        A.DoingBusinessAs,
        A.LinkedInUrl,
        A.Website,
        A.Country,
        A.State,
        A.County,
        A.City,
        A.StreetAddress1,
        A.StreetAddress2,
        A.Zip,
        A.Phone,
        A.Extension,
        A.Fax,
        A.Address,
        C.State as ClientState,
        C.NaicsCode as ClientNaicsCode,
        R.State as ReferralState,
        R.NaicsCode as ReferralNaicsCode,
        A.CreatedDateTimeUtc,
        A.LastModifiedDateTimeUtc,
        A.IsDeleted,
        JSON_ARRAY('Accounts.Read', 'Accounts.ReadWrite', 'Accounts.ReadExport') as Permissions,
        'account' as EntityType
    FROM Accounts A
        LEFT JOIN Clients c on A.Id = C.AccountId
        LEFT JOIN Referrals r on A.Id = R.AccountId
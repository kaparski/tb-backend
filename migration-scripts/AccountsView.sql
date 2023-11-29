CREATE OR ALTER VIEW AccountsView AS
SELECT a.Id,
       a.TenantId,
       a.Name,
       a.AccountId,
       a.State,
       a.City,
       a.Country,
       a.County,
       a.Address,
       a.Address1,
       a.Address2,
       a.DoingBusinessAs,
       a.LinkedInUrl,
       a.Website,
       a.Zip,
       a.CreatedDateTimeUtc,
       a.LastModifiedDateTimeUtc,
       a.IsDeleted,
       a.DeletedDateTimeUtc,
       c.State as ClientState,
       c.Status as ClientStatus,
       c.AnnualRevenue as AnnualRevenue,
       c.FoundationYear as FoundationYear,
       c.EmployeeCount as EmployeeCount,
       c.PrimaryContactId as ClientPrimaryContactId,
       r.PrimaryContactId as ReferralPrimaryContactId,
       ClientPrimaryContact,
	     ReferralPrimaryContact,
       Salespersons,
       r.State as ReferralState,
       r.Status as ReferralStatus,
	     r.OrganizationType,
	     r.Type as ReferralType,
       ClientAccountManagers,
	     ReferralAccountManagers,
       CAST(nc.Code AS NVARCHAR(6)) AS NaicsCode,
       nc.Title AS NaicsCodeIndustry,
       CONCAT_WS(', ',
                 NULLIF(CONCAT_WS(' - ', c.State, c.Status), ''),
                 NULLIF(CONCAT_WS(' - ', r.State, r.Status), '')) as AccountType

FROM Accounts a
         LEFT JOIN Clients c on a.Id = c.AccountId
         LEFT JOIN [NaicsCodes] AS [nc] ON [a].[PrimaryNaicsCode] = [nc].[Code]
         LEFT JOIN Referrals r on a.Id = r.AccountId
         OUTER APPLY(
    SELECT
        CONCAT_WS(' ', contacts.FirstName, contacts.LastName) as [ClientPrimaryContact]
    FROM
        [Contacts] as [contacts]
    WHERE
        (
                    [contacts].TenantId = a.TenantId
                AND [contacts].Id = c.PrimaryContactId)
) cpc(ClientPrimaryContact)
         OUTER APPLY(
    SELECT
        CONCAT_WS(' ', contacts.FirstName, contacts.LastName) as [ReferralPrimaryContact]
    FROM
        [Contacts] as [contacts]
    WHERE
        (
                    [contacts].TenantId = a.TenantId
                AND [contacts].Id = r.PrimaryContactId)
) rpc(ReferralPrimaryContact)
         OUTER APPLY (
    SELECT
        STRING_AGG(u.FullName, ', ') WITHIN GROUP (ORDER BY u.FullName ASC) AS [ClientAccountManagers]
    FROM
        ClientManagers AS [cm]
            JOIN [Users] AS [u] ON [u].Id = [cm].UserId
    WHERE
        (
                ([u].[IsDeleted] IS NULL)
                OR [u].[IsDeleted] = CAST(0 AS bit)
            )
      AND [cm].AccountId = [a].Id
) cm(ClientAccountManagers)
OUTER APPLY (
    SELECT
        STRING_AGG(u.FullName, ', ') WITHIN GROUP (ORDER BY u.FullName ASC) AS [ReferralAccountManagers]
    FROM
        ReferralManagers AS [rm]
            JOIN [Users] AS [u] ON [u].Id = [rm].UserId
    WHERE
        (
                ([u].[IsDeleted] IS NULL)
                OR [u].[IsDeleted] = CAST(0 AS bit)
            )
      AND [rm].AccountId = [a].Id
) rm(ReferralAccountManagers)
         OUTER APPLY (
    SELECT
        STRING_AGG(u.FullName, ', ') WITHIN GROUP (ORDER BY u.FullName ASC) AS [Salespersons]
    FROM
        Salespersons AS [sp]
            JOIN [Users] AS [u] ON [u].Id = [sp].UserId
    WHERE
        (
                ([u].[IsDeleted] IS NULL)
                OR [u].[IsDeleted] = CAST(0 AS bit)
            )
      AND [sp].AccountId = [a].Id
) sp(Salespersons)

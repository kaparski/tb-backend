
CREATE 
OR ALTER VIEW ReferralsView AS 
SELECT 
  [r].[TenantId],
  [r].[AccountId],
  [a].[AccountId] as [AccountIdField],
  [a].[State],
  [a].[Name], 
  [a].[City], 
  [pc].[FullName] AS [PrimaryContactName],
  [r].[State] AS [ReferralState], 
  [r].[Status], 
  [r].[DaysOpen], 
  [Salespersons],
  [ReferralManagers] as [AccountManagers],
  [r].[CreatedDateTimeUtc],
  [r].[DeactivationDateTimeUtc],
  [r].[ReactivationDateTimeUtc],
  [r].[LastModifiedDateTimeUtc],
  [r].[DeletedDateTimeUtc],
  [r].[OrganizationType],
  [r].[Type],
  [a].[Country],
  [a].[County],
  [r].[PrimaryContactId],
  CAST([nc].[Code] AS VARCHAR(6)) AS [NaicsCode],
  [nc].[Title] AS [NaicsCodeIndustry],
  [r].[IsDeleted]
FROM 
  [Referrals] AS [r] 
  INNER JOIN [Accounts] AS [a] ON [r].[AccountId] = [a].[Id]
  LEFT JOIN [NaicsCodes] AS [nc] ON [a].[PrimaryNaicsCode] = [nc].[Code]
  LEFT JOIN [Contacts] AS [pc] ON [r].[PrimaryContactId] = [pc].[Id]
  OUTER APPLY (
    SELECT 
      STRING_AGG(u.FullName, ', ') AS [Salespersons] 
    FROM 
	  [Salespersons] AS [s]
      JOIN [Users] AS [u] ON [u].Id = [s].UserId 
    WHERE 
      (
        ([u].[IsDeleted] IS NULL) 
        OR [u].[IsDeleted] = CAST(0 AS bit)
      ) 
      AND [s].AccountId = [a].Id
  ) u(Salespersons)
  OUTER APPLY (
    SELECT 
      STRING_AGG(u.FullName, ', ') AS [ReferralManagers]
    FROM 
      ReferralManagers AS [rm]
      JOIN [Users] AS [u] ON [u].Id = [rm].UserId 
    WHERE 
      (
        ([u].[IsDeleted] IS NULL) 
        OR [u].[IsDeleted] = CAST(0 AS bit)
      ) 
      AND [rm].AccountId = [a].Id
  ) rm(ReferralManagers)


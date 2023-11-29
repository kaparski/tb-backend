
CREATE 
OR ALTER VIEW ClientsView AS 
SELECT
  [a].[AccountId] as [AccountIdField],
  [c].[TenantId],
  [c].[AccountId], 
  [a].[State],
  [a].[Name], 
  [a].[City], 
  [pc].[FullName] AS [PrimaryContactName],
  [c].[State] AS [ClientState], 
  [c].[Status], 
  [c].[DaysOpen], 
  [Salespersons],
  [ClientManagers] as [AccountManagers],
  [c].[CreatedDateTimeUtc],
  [c].[DeactivationDateTimeUtc],
  [c].[ReactivationDateTimeUtc],
  [c].[LastModifiedDateTimeUtc],
  [c].[DeletedDateTimeUtc],
  [a].[Country],
  [a].[County],
  [c].[EmployeeCount],
  [c].[AnnualRevenue],
  [c].[PrimaryContactId],
  CAST([nc].[Code] AS VARCHAR(6)) AS [NaicsCode],
  [nc].[Title] AS [NaicsCodeIndustry],
  [c].[IsDeleted]
FROM 
  [Clients] AS [c] 
  INNER JOIN [Accounts] AS [a] ON [c].[AccountId] = [a].[Id]
  LEFT JOIN [NaicsCodes] AS [nc] ON [a].[PrimaryNaicsCode] = [nc].[Code]
  LEFT JOIN [Contacts] AS [pc] ON [c].[PrimaryContactId] = [pc].[Id]
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
      STRING_AGG(u.FullName, ', ') AS [ClientManagers]
    FROM 
      ClientManagers AS [cm]
      JOIN [Users] AS [u] ON [u].Id = [cm].UserId 
    WHERE 
      (
        ([u].[IsDeleted] IS NULL) 
        OR [u].[IsDeleted] = CAST(0 AS bit)
      ) 
      AND [cm].AccountId = [a].Id
  ) cm(ClientManagers)


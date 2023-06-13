CREATE OR ALTER VIEW AccountsView AS
SELECT a.Id,
       a.TenantId,
       a.Name,
       a.State,
       a.City,
       a.CreatedDateTimeUtc,
       a.LastModifiedDateTimeUtc,
       a.IsDeleted,
       a.DeletedDateTimeUtc,
       CONCAT_WS(',',
                 NULLIF(CONCAT_WS(' - ', c.State, c.IsEnabled), ''),
                 NULLIF(CONCAT_WS(' - ', r.State, r.isEnabled), '')) as AccountType
FROM Accounts a
         LEFT JOIN Clients c on a.Id = c.AccountId
         LEFT JOIN Referrals r on a.Id = r.AccountId;

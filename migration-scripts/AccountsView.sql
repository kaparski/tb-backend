CREATE OR ALTER VIEW AccountsView AS
SELECT a.Id,
       a.TenantId,
       a.Name,
       a.State,
       a.City,
       a.Website,
       a.CreatedDateTimeUtc,
       a.LastModifiedDateTimeUtc,
       a.IsDeleted,
       a.DeletedDateTimeUtc,
       c.State as ClientState,
       c.Status as ClientStatus,
       r.State as ReferralState,
       r.Status as ReferralStatus,
       CONCAT_WS(',',
                 NULLIF(CONCAT_WS(' - ', c.State, c.Status), ''),
                 NULLIF(CONCAT_WS(' - ', r.State, r.Status), '')) as AccountType
FROM Accounts a
         LEFT JOIN Clients c on a.Id = c.AccountId
         LEFT JOIN Referrals r on a.Id = r.AccountId;

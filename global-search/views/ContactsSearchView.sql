CREATE OR ALTER VIEW ContactsSearchView AS
  SELECT
    CONCAT_WS('_', C.Id, C.TenantId) AS DocumentId,
    Id AS OriginalId,
    TenantId AS TenantId,
    Email,
	FullName,
    SecondaryEmail,
    JobTitle,
    CreatedDateTimeUtc,
    LastModifiedDateTimeUtc,
    IsDeleted,
    JSON_ARRAY(
      'Contacts.Read',
      'Contacts.ReadWrite',
      'Contacts.ReadExport',
      'Contacts.Activation') as Permissions,
    'contact' AS EntityType
  FROM Contacts AS C

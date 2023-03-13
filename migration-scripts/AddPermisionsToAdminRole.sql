DECLARE @adminRolePermissions TABLE (NAME NVARCHAR(250));

DECLARE @tenantId UNIQUEIDENTIFIER,
        @adminRoleId UNIQUEIDENTIFIER;

SELECT @tenantId = Id
FROM Tenants
WHERE Name ='CTI'

SELECT @adminRoleId = Id
FROM ROLES r
JOIN TenantRoles tr ON tr.RoleId = r.Id
AND tr.TenantId = @tenantId AND r.Name = 'Admin'

INSERT INTO @adminRolePermissions (Name)
VALUES ('Users.Read'),
       ('Users.ReadWrite'),
       ('Users.ReadExport'),
       ('Roles.Read')

INSERT INTO Permissions (Id, Name, CreatedDateUtc)
SELECT NEWID(),
       arp.Name,
       GETUTCDATE()
FROM @adminRolePermissions arp
WHERE NOT EXISTS
    (SELECT 1
     FROM Permissions
     WHERE Name = arp.Name)

INSERT INTO TenantPermissions (TenantId, PermissionId)
SELECT @tenantId,
       Id
FROM Permissions p
WHERE NOT EXISTS
    (SELECT 1
     FROM TenantPermissions
     WHERE TenantId = @tenantId
       AND PermissionId = p.Id)

INSERT INTO TenantRolePermissions (TenantId, RoleId, PermissionId)
SELECT @tenantId,
       @adminRoleId,
       Id
FROM Permissions p
WHERE p.Name in
    (SELECT Name
     FROM @adminRolePermissions)
  AND NOT EXISTS
    (SELECT 1
     FROM TenantRolePermissions
     WHERE TenantId = @tenantId
       AND RoleId = @adminRoleId
       AND PermissionId = p.Id)

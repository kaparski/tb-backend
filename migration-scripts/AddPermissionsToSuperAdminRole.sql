DECLARE @superAdminRolePermissions TABLE (NAME NVARCHAR(250));
DECLARE @superAdminRoleId UNIQUEIDENTIFIER;
DECLARE @superAdminRoleName AS NVARCHAR(100) = N'Super admin';

SELECT @superAdminRoleId = Id
FROM ROLES
where Name = @superAdminRoleName

INSERT INTO @superAdminRolePermissions
    (Name)
VALUES
    ('Tenants.Read'),
    ('Tenants.ReadWrite'),
    ('Tenants.ReadExport'),
    ('Programs.Read'),
    ('Programs.ReadWrite'),
    ('Programs.ReadExport')

INSERT INTO Permissions (Id, Name, CreatedDateTimeUtc)
SELECT NEWID(),
       arp.Name,
       GETUTCDATE()
FROM @superAdminRolePermissions arp
WHERE NOT EXISTS
    (SELECT 1
     FROM Permissions
     WHERE Name = arp.Name)

INSERT INTO RolePermissions(RoleId, PermissionId)
SELECT @superAdminRoleId, Id
FROM Permissions p
WHERE p.Name in
    (SELECT Name FROM @superAdminRolePermissions)
    AND NOT EXISTS(SELECT 1 FROM RolePermissions WHERE RoleId = @superAdminRoleId AND PermissionId = p.Id);
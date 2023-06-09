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
VALUES ('Divisions.Activation'),
       ('Divisions.Read'),
       ('Divisions.ReadWrite'),
       ('Divisions.ReadExport'),
       ('Departments.Read'),
       ('Departments.ReadWrite'),
       ('Departments.ReadExport'),
       ('JobTitles.Read'),
       ('JobTitles.ReadWrite'),
       ('JobTitles.ReadExport'),
       ('Programs.Read'),
       ('Programs.ReadExport'),
       ('Roles.Read'),
       ('Roles.ReadWrite'),
       ('ServiceAreas.Read'),
       ('ServiceAreas.ReadWrite'),
       ('ServiceAreas.ReadExport'),
       ('TableFilters.Read'),
       ('TableFilters.ReadWrite'),
       ('Teams.Read'),
       ('Teams.ReadWrite'),
       ('Teams.ReadExport'),
       ('Users.Read'),
       ('Users.ReadWrite'),
       ('Users.ReadExport'),
       ('Accounts.Read')

INSERT INTO Permissions (Id, Name, CreatedDateTimeUtc)
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

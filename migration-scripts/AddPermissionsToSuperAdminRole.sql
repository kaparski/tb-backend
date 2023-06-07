DECLARE @superAdminRolePermissions TABLE (NAME NVARCHAR(250));
DECLARE @superAdminRoleId UNIQUEIDENTIFIER;
DECLARE @superAdminRoleName AS NVARCHAR(100) = N'Super admin';

SELECT @superAdminRoleId = Id
FROM ROLES
where Name = @superAdminRoleName

INSERT INTO @superAdminRolePermissions
    (Name)
VALUES ('Departments.Read'),
       ('Departments.ReadWrite'),
       ('Departments.ReadExport'),
       ('Divisions.Activation'),
       ('Divisions.Read'),
       ('Divisions.ReadWrite'),
       ('Divisions.ReadExport'),
       ('JobTitles.Read'),
       ('JobTitles.ReadWrite'),
       ('JobTitles.ReadExport'),
       ('Programs.Read'),
       ('Programs.ReadWrite'),
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
       ('Tenants.Read'),
       ('Tenants.ReadWrite'),
       ('Tenants.ReadExport'),
       ('Users.Read'),
       ('Users.ReadWrite'),
       ('Users.ReadExport')
       
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

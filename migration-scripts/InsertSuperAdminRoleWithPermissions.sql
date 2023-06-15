DECLARE @rolePermissions TABLE
                         (
                             NAME NVARCHAR(250)
                         );
DECLARE @roleType AS INT = 1;
DECLARE @roleId AS UNIQUEIDENTIFIER = NEWID();
DECLARE @roleName AS NVARCHAR(100) = N'Super admin';

IF NOT EXISTS(SELECT 1
              FROM Roles
              WHERE Name = @roleName)
    BEGIN
        INSERT INTO Roles(Id, Name, Type) VALUES (@roleId, @roleName, @roleType);
    END;
ELSE
    BEGIN
        SELECT @roleId = Id FROM Roles WHERE Name = @roleName;
    END;

INSERT INTO @rolePermissions
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
       srp.Name,
       GETUTCDATE()
FROM @rolePermissions srp
WHERE NOT EXISTS
    (SELECT 1
     FROM Permissions
     WHERE Name = srp.Name)

INSERT INTO RolePermissions(RoleId, PermissionId)
SELECT @roleId, Id
FROM Permissions p
WHERE p.Name in
      (SELECT Name FROM @rolePermissions)
  AND NOT EXISTS(SELECT 1 FROM RolePermissions WHERE RoleId = @roleId AND PermissionId = p.Id);

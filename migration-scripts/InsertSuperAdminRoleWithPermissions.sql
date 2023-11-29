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
       ('Programs.ReadTenantOrgUnits'),
       ('Programs.ReadAssignTenantOrgUnits'),
       ('Programs.ReadActivation'),
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
       ('Tenants.Switch'),
       ('Users.Read'),
       ('Users.ReadWrite'),
       ('Users.ReadExport'),
       ('Accounts.Read'),
       ('Accounts.ReadWrite'),
       ('Accounts.ReadExport'),
       ('Prospects.Activation'),
       ('Contacts.Read'),
       ('Contacts.ReadWrite'),
       ('Contacts.ReadExport'),
       ('Contacts.Activation'),
       ('Locations.Read'),
       ('Locations.ReadWrite'),
       ('Locations.ReadExport'),
       ('Locations.ReadActivation'),
       ('Entities.Read'),
       ('Entities.ReadWrite'),
       ('Entities.ReadExport'),
       ('Entities.ReadActivation'),
       ('Clients.Read'),
       ('Clients.ReadWrite'),
       ('Clients.ReadExport'),
       ('Clients.Activation'),
       ('Referrals.Read'),
       ('Referrals.ReadWrite'),
       ('Referrals.ReadExport'),
       ('Referrals.Activation'),
       ('Accounts.AssignContact'),
       ('Contacts.AssignContact'),
       ('Documents.Read'),
       ('Documents.ReadExport'),
       ('Templates.Read');

IF DB_NAME() IN ('qa-taxbeacon', 'dev-taxbeacon')
  BEGIN
    INSERT INTO @rolePermissions (Name) VALUES ('QualityAssurance.Full');
  END


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

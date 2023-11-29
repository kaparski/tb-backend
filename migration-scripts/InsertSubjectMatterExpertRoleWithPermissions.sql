--- Insert Roles and Permission
--- Assign Permissions to Role

DECLARE @subjectMatterExpertPermissions TABLE
                                        (
                                          NAME NVARCHAR(250)
                                        );
DECLARE @roleId UNIQUEIDENTIFIER = NEWID();
DECLARE @tenantId AS UNIQUEIDENTIFIER;
DECLARE @roleName AS NVARCHAR(100) = N'Subject Matter Expert'

BEGIN TRANSACTION [Tran1];
BEGIN TRY
  SELECT @tenantId = Id
  FROM Tenants
  WHERE Name = 'Corporate Tax Incentives'

  IF NOT EXISTS(SELECT Id FROM Roles WHERE Name = @roleName)
    BEGIN
      INSERT INTO Roles(Id, Name, CreatedDateTimeUtc) VALUES (@roleId, @roleName, GETUTCDATE());
    END;
  ELSE
    BEGIN
      SELECT @roleId = Id FROM ROLES where Name = @roleName;
    END;

  IF NOT EXISTS(SELECT RoleId FROM TenantRoles WHERE RoleId = @roleId AND TenantId = @tenantId)
    BEGIN
      INSERT INTO TenantRoles VALUES (@tenantId, @roleId);
    END;

  INSERT INTO @subjectMatterExpertPermissions (Name)
  VALUES ('Accounts.Read'),
         ('Accounts.ReadWrite'),
         ('Accounts.ReadExport'),
         ('Clients.Read'),
         ('Clients.ReadWrite'),
         ('Clients.ReadExport'),
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
         ('Entities.Import'),
         ('TableFilters.Read'),
         ('TableFilters.ReadWrite'),
         ('Programs.Read'),
         ('Accounts.AssignContact'),
         ('Contacts.AssignContact'),
         ('Documents.Read'),
         ('Documents.ReadExport'),
         ('Templates.Read'),
         ('Referrals.ReadWrite');

  IF DB_NAME() IN ('qa-taxbeacon', 'dev-taxbeacon')
    BEGIN
      INSERT INTO @subjectMatterExpertPermissions (Name) VALUES ('QualityAssurance.Full');
    END

  INSERT INTO Permissions (Id, Name, CreatedDateTimeUtc)
  SELECT NEWID(),
         smep.Name,
         GETUTCDATE()
  FROM @subjectMatterExpertPermissions smep
  WHERE NOT EXISTS
          (SELECT 1
           FROM Permissions
           WHERE Name = smep.Name);

  INSERT INTO TenantPermissions (TenantId, PermissionId)
  SELECT @tenantId,
         Id
  FROM Permissions p
  WHERE NOT EXISTS
          (SELECT 1
           FROM TenantPermissions
           WHERE TenantId = @tenantId
             AND PermissionId = p.Id);

  INSERT INTO TenantRolePermissions (TenantId, RoleId, PermissionId)
  SELECT @tenantId,
         @roleId,
         Id
  FROM Permissions p
  WHERE p.Name in
        (SELECT Name
         FROM @subjectMatterExpertPermissions)
    AND NOT EXISTS
    (SELECT 1
     FROM TenantRolePermissions
     WHERE TenantId = @tenantId
       AND RoleId = @roleId
       AND PermissionId = p.Id);

  DELETE
  FROM TenantRolePermissions
  WHERE RoleId = @roleId
    AND TenantId = @tenantId
    AND PermissionId NOT IN (select p.Id
                             FROM Permissions p
                                    JOIN @subjectMatterExpertPermissions pn on p.Name = pn.Name);

  COMMIT TRANSACTION [Tran1];
END TRY
BEGIN CATCH
  ROLLBACK TRANSACTION [Tran1]
END CATCH

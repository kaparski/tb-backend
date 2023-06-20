--- Insert Roles and Permission
--- Assign Permissions to Role

DECLARE @projectManagerPermissions TABLE
                                 (
                                     NAME NVARCHAR(250)
                                 );
DECLARE @roleId UNIQUEIDENTIFIER = NEWID();
DECLARE @tenantId AS UNIQUEIDENTIFIER;
DECLARE @roleName AS NVARCHAR(100) = N'Project Manager'

BEGIN TRANSACTION [Tran1];
BEGIN TRY
    SELECT @tenantId = Id
    FROM Tenants
    WHERE Name = 'CTI'

    IF NOT EXISTS(SELECT Id FROM Roles WHERE Name = @roleName)
        BEGIN
            INSERT INTO Roles(Id, Name, CreatedDateTimeUtc) VALUES (@roleId, @roleName, GETUTCDATE());
        END;
    ELSE
        BEGIN
            SELECT @roleId = Id
            FROM ROLES r
                     JOIN TenantRoles tr ON tr.RoleId = r.Id
                AND tr.TenantId = @tenantId AND r.Name = @roleName
        END;

    IF NOT EXISTS(SELECT RoleId FROM TenantRoles WHERE RoleId = @roleId AND TenantId = @tenantId)
        BEGIN
            INSERT INTO TenantRoles VALUES (@tenantId, @roleId);
        END;

    INSERT INTO @projectManagerPermissions (Name)
    VALUES ('Accounts.Read'),
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
           ('Locations.Activation'),
           ('Entities.Read'),
           ('Entities.ReadWrite'),
           ('Entities.ReadExport'),
           ('Entities.Activation'),
           ('TableFilters.Read'),
           ('TableFilters.ReadWrite');

    INSERT INTO Permissions (Id, Name, CreatedDateTimeUtc)
    SELECT NEWID(),
           pmp.Name,
           GETUTCDATE()
    FROM @projectManagerPermissions pmp
    WHERE NOT EXISTS
        (SELECT 1
         FROM Permissions
         WHERE Name = pmp.Name)

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
           @roleId,
           Id
    FROM Permissions p
    WHERE p.Name in
          (SELECT Name
           FROM @projectManagerPermissions)
      AND NOT EXISTS
        (SELECT 1
         FROM TenantRolePermissions
         WHERE TenantId = @tenantId
           AND RoleId = @roleId
           AND PermissionId = p.Id)

    COMMIT TRANSACTION [Tran1];
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION [Tran1]
END CATCH

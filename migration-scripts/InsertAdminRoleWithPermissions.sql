﻿--- Insert Roles and Permission
--- Assign Roles to all users in tenant
--- Assign Permissions to Role

DECLARE @adminRolePermissions TABLE (NAME NVARCHAR(250));
DECLARE @roleId UNIQUEIDENTIFIER = NEWID();
DECLARE @tenantId AS UNIQUEIDENTIFIER;
DECLARE @roleName AS NVARCHAR(100) = N'Admin'

BEGIN TRANSACTION [Tran1];
BEGIN TRY
	SELECT @tenantId = Id
	FROM Tenants
	WHERE Name ='CTI'

	IF NOT EXISTS(SELECT Id FROM Roles WHERE Id = @roleId)
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

	  INSERT INTO TenantUsers
		SELECT Id, @tenantId
		FROM Users AS u
		WHERE NOT EXISTS(SELECT UserId
						 FROM TenantUsers
						 WHERE UserId = u.Id
						 AND TenantId = @tenantId);

    INSERT INTO TenantUserRoles
    SELECT TenantId, @roleId, UserId
    FROM TenantUsers AS tu
    WHERE NOT EXISTS(SELECT UserId
                     FROM TenantUserRoles
                     WHERE UserId = tu.UserId
                     AND TenantId = tu.TenantId
                     AND RoleId = @roleId);

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
         ('Users.ReadExport')

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
		   @roleId,
		   Id
	FROM Permissions p
	WHERE p.Name in
		(SELECT Name
		 FROM @adminRolePermissions)
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

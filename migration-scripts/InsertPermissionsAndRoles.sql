--- Insert Roles and Permission
--- Assign Roles to all users in tenant
--- Assign Permissions to Role

DECLARE @TenantId AS UNIQUEIDENTIFIER
SELECT TOP 1 @TenantId = Id FROM Tenants
DECLARE @RoleId AS UNIQUEIDENTIFIER = NEWID()
DECLARE @RoleName AS NVARCHAR(100) = N'Admin'

BEGIN TRANSACTION [Tran1];
BEGIN TRY
  IF NOT EXISTS(SELECT Id FROM Roles WHERE Id = @RoleId)
  BEGIN
    INSERT INTO Roles(Id, Name, CreatedDateUtc) VALUES (@RoleId, @RoleName, GETUTCDATE());
  END;

  IF NOT EXISTS(SELECT RoleId FROM TenantRoles WHERE RoleId = @RoleId AND TenantId = @TenantId)
  BEGIN
    INSERT INTO TenantRoles VALUES (@TenantId, @RoleId);
  END;

  INSERT INTO TenantUsers
  SELECT Id, @TenantId
  FROM Users AS u
  WHERE NOT EXISTS(SELECT UserId
                   FROM TenantUsers
                   WHERE UserId = u.Id
                   AND TenantId = @TenantId);

  INSERT INTO TenantUserRoles
  SELECT TenantId, @RoleId, UserId
  FROM TenantUsers AS tu
  WHERE NOT EXISTS(SELECT UserId
                   FROM TenantUserRoles
                   WHERE UserId = tu.UserId
                   AND TenantId = tu.TenantId
                   AND RoleId = @RoleId);

  COMMIT TRANSACTION [Tran1];
END TRY
BEGIN CATCH
  ROLLBACK TRANSACTION [Tran1]
END CATCH

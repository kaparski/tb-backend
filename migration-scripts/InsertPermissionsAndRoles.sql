--- Insert Roles and Permission
--- Assign Roles to all users in tenant
--- Assign Permissions to Role

DECLARE @TenantId AS UNIQUEIDENTIFIER
SELECT TOP 1 @TenantId = Id FROM Tenants
DECLARE @RoleId AS UNIQUEIDENTIFIER = 'A11BEFD4-92A8-4ABE-9AE9-F3F29E1BB22C'
DECLARE @RoleName AS NVARCHAR(100) = N'Admin'
DECLARE @Permissions TABLE (Id UNIQUEIDENTIFIER, Name NVARCHAR(100))
INSERT INTO @Permissions VALUES('B0E3DAB5-8C55-4151-A6CF-12D25FF4F3C3',N'Login'),
                               ('7DA1C5E5-5882-457B-8948-59254258188E',N'CreateUser'),
                               ('2967C2D3-2BEC-4408-A904-853C389FA916',N'ReadListOfUsers'),
                               ('37069740-667E-403E-B251-B54301D106FD',N'ReadUserDetails'),
                               ('3D2A2949-CBF7-4FCF-B421-4E77A6E57857',N'UpdateUserStatus')

BEGIN TRANSACTION [Tran1];
BEGIN TRY
INSERT INTO Permissions SELECT Id, Name FROM @Permissions
  INSERT INTO Roles VALUES (@RoleId, @RoleName)
                        INSERT INTO TenantRoles VALUES (@RoleId, @TenantId)
                        INSERT INTO TenantPermissions SELECT @TenantId, Id FROM @Permissions
  INSERT INTO TenantRolePermissions SELECT @RoleId, @TenantId, Id FROM @Permissions
  INSERT INTO TenantRoleUsers SELECT @RoleId, TenantId, UserId FROM TenantUsers
  COMMIT TRANSACTION [Tran1]
END TRY
BEGIN CATCH
ROLLBACK TRANSACTION [Tran1]
END CATCH

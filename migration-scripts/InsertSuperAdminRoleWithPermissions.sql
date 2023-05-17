DECLARE @superAdminRoleName AS NVARCHAR(100) = N'Super admin';
DECLARE @roleType AS INT = 1;
DECLARE @roleId AS UNIQUEIDENTIFIER = NEWID()

IF NOT EXISTS(SELECT 1
              FROM Roles
              WHERE Name = @superAdminRoleName)
    BEGIN
        INSERT INTO Roles(Id, Name, Type) VALUES (@roleId, @superAdminRoleName, @roleType);
    END;
ELSE
    BEGIN
        SELECT @roleId = Id FROM Roles WHERE Name = @superAdminRoleName;
    END;

INSERT INTO RolePermissions(RoleId, PermissionId)
SELECT @roleId, Id
FROM Permissions p

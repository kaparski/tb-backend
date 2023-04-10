DECLARE @superAdminRoleName AS NVARCHAR(100) = N'Super admin';
DECLARE @roleId AS UNIQUEIDENTIFIER = NEWID()

IF NOT EXISTS(SELECT 1
              FROM Roles
              WHERE Name = @superAdminRoleName)
    BEGIN
        INSERT INTO Roles(Id, Name) VALUES (@roleId, @superAdminRoleName);
    END;
ELSE
    BEGIN
        SELECT @roleId = Id FROM Roles WHERE Name = @superAdminRoleName;
    END;

INSERT INTO RolePermissions(RoleId, PermissionId)
SELECT @roleId, Id
FROM Permissions p
WHERE NOT EXISTS(SELECT 1 FROM RolePermissions WHERE RoleId = @roleId AND PermissionId = p.Id);
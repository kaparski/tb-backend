
DROP VIEW IF EXISTS dbo.UsersView
GO

CREATE VIEW dbo.UsersView AS
	select 
		u.Id,
		u.FirstName,
		u.LastName,
		u.Email,
		u.Status,
		u.CreatedDateTimeUtc,
		u.LastModifiedDateTimeUtc,
		u.LastLoginDateTimeUtc,
		u.DeactivationDateTimeUtc,
		u.ReactivationDateTimeUtc,
		u.FullName,
		u.LegalName,
		u.DivisionId,
		di.Name as Division,
		u.DepartmentId,
		d.Name as Department,
		u.JobTitleId,
		jt.Name as JobTitle,
		u.TeamId,
		t.Name as TeamName,
		tu.TenantId,
		CAST(u.Id as varchar(50)) + ISNULL(CAST(tu.TenantId as varchar(50)), '') as UserIdPlusTenantId,
		STRING_AGG(r.Name, ', ') as Roles,
		STRING_AGG(r.Name, '|') as RoleNamesAsString,
		STRING_AGG(CAST(r.Id as varchar(255)), '|') as RoleIdsAsString
	from 
		dbo.Users u
		left join
		dbo.TenantUsers tu
		on
		u.Id = tu.UserId
		left join
		dbo.TenantUserRoles tur
		on
		tur.UserId = u.Id and tur.TenantId = tu.TenantId
		left join
		dbo.Roles r
		on
		r.Id = tur.RoleId
		left join
		dbo.Divisions di
		on
		u.DivisionId = di.Id
		left join
		dbo.Departments d
		on
		u.DepartmentId = d.Id
		left join
		dbo.JobTitles jt
		on
		u.JobTitleId = jt.Id
		left join
		dbo.Teams t
		on
		u.TeamId = t.Id
	where
		u.IsDeleted IS NULL OR u.IsDeleted = CAST(0 AS bit)
	group by
		u.Id,
		u.FirstName,
		u.LastName,
		u.Email,
		u.Status,
		u.CreatedDateTimeUtc,
		u.LastModifiedDateTimeUtc,
		u.LastLoginDateTimeUtc,
		u.DeactivationDateTimeUtc,
		u.ReactivationDateTimeUtc,
		u.FullName,
		u.LegalName,
		u.DivisionId,
		u.DepartmentId,
		u.JobTitleId,
		u.TeamId,
		di.Name,
		d.Name,
		jt.Name,
		t.Name,
		tu.TenantId
GO
